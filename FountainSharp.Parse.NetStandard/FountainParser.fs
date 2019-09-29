﻿// --------------------------------------------------------------------------------------
// F# Markdown (MarkdownParser.fs)
// (c) Tomas Petricek, 2012, Available under Apache 2.0 license.
// --------------------------------------------------------------------------------------
// Modifications (c) by Bryan Costanich, 2015, 2016
// --------------------------------------------------------------------------------------

module internal FountainSharp.Parse.Parser

open System
open System.Text.RegularExpressions

open FSharp.Collections
open FountainSharp
open FountainSharp.Parse.Collections
open FountainSharp.Parse.Patterns
open FountainSharp.Parse.Patterns.List
open FountainSharp.Parse.Helper

let printDebug fmt par =
    let s = FSharp.Core.Printf.sprintf fmt par
    System.Diagnostics.Debug.WriteLine s

[<Literal>]
let EmptyLine = ""

// Often used properties of a match for blocks and spans
// Used for range calculation. Generic as the result can be String, char list, etc.
type MatchResult<'T> = 
    {
      Text   : 'T; // recognized text
      Length : int; // the length of input recognized
      Offset : int // offset from the beginning of the input
    }

/// Defines a context for the parsing
type ParsingContext(?newline : string, ?position : int, ?lastParsedBlock : FountainBlockElement option, ?treatDoubleSpaceAsNewLine : bool, ?preserveIndenting : bool) =
    let newLine = defaultArg newline Environment.NewLine
    let lastParsedBlock : FountainBlockElement option = defaultArg lastParsedBlock None
    let position = defaultArg position 0
    let doubleSpaceAsNewLine : bool = defaultArg treatDoubleSpaceAsNewLine false
    let preserveIndenting : bool = defaultArg preserveIndenting false

    /// New line literal
    member this.NewLine with get() = newLine

    /// The block that has been parsed before the current block
    member ctx.LastParsedBlock with get() = lastParsedBlock

    /// The position in the input where the block currently being parsed begins
    member ctx.Position with get() = position

    /// Gets whether to treat '  ' as new line
    member ctx.TreatDoubleSpaceAsNewLine with get() = doubleSpaceAsNewLine

    /// Gets whether we should preserve white spaces or not
    member ctx.PreserveIndenting with get() = preserveIndenting

    /// Offsets the current position
    member ctx.Offset(length) =
        new ParsingContext(ctx.NewLine, ctx.Position + length, ctx.LastParsedBlock, ctx.TreatDoubleSpaceAsNewLine, ctx.PreserveIndenting)
    
    /// Sets the previously parsed block type
    member ctx.WithLastParsedBlock(block : FountainBlockElement option) =
        new ParsingContext(ctx.NewLine, ctx.Position, block, ctx.TreatDoubleSpaceAsNewLine, ctx.PreserveIndenting)
    
    /// Enable or disable treating '  ' as new line
    member ctx.WithDoubleSpaceAsNewLine(enable) =
        new ParsingContext(ctx.NewLine, ctx.Position, ctx.LastParsedBlock, enable, ctx.PreserveIndenting)
    
    /// Enable or disable preserving white space
    member ctx.WithPreserveIndenting(enable) =
        new ParsingContext(ctx.NewLine, ctx.Position, ctx.LastParsedBlock, ctx.TreatDoubleSpaceAsNewLine, enable)

    /// Gets the last position of last parsed block's range. When there is no last parsed block, it returns 0
    member ctx.LastParsedLocation
      with get() =
        match ctx.LastParsedBlock with
        | None -> 0
        | Some(block) -> block.Range.EndLocation

//====== Parser
// Part 1: Inline Formatting

/// Succeeds when the specified character list starts with an escaped 
/// character - in that case, returns the character and the tail of the list
// TODO: some of these should be stripped out.
let inline (|EscapedChar|_|) input = 
  match input with
  | '\\'::( ( '*' | '\\' | '`' | '_' | '{' | '}' | '[' | ']' 
            | '(' | ')' | '>' | '#' | '.' | '!' | '+' | '-' | '$') as c) ::rest -> Some(c, rest)
  | _ -> None

/// matches a delimited list of characters that starts with some sub list, called the 
/// delimiter bracket, (like ['*'; '*'] for bold) that occurs at the beginning and the end of the text.
/// returns a wrapped list and then the rest of the characters after the delimited text.
let (|DelimitedText|_|) delimiterBracket input =
  let endl = delimiterBracket // end limiter
  // Like List.partitionUntilEquals, but skip over escaped characters
  let rec loop acc found input =
    match input with
    | EscapedChar(x, xs) -> loop (x::'\\'::acc) found xs // skip the escaped char
    | x::xs -> 
      if List.startsWith endl input && Char.IsWhiteSpace acc.Head = false then
        loop (x::acc) true xs // found a delimiter, but we have to found the furthermost
      else
        if found then // now not matching, but in the previous iteration it was ok
          Some(List.rev acc.Tail, List.last acc :: input)
        else
          loop (x::acc) found xs // search further
    | _ when found -> Some(List.rev acc.Tail, List.last acc :: input) // input has 1 or zero elements and previous iteration matched
    | [] -> None // not found the delimiter and traversed the list
  // If it starts with 'delimiterBracket', let's search for 'endl'
  if List.startsWith delimiterBracket input then
    match loop [] false (List.skip delimiterBracket.Length input) with
    | Some(pre, post) -> Some(pre, List.skip delimiterBracket.Length post)
    | None -> None
  else None

/// recognizes emphasized text of Italic, Bold, etc.
/// take something like "*some text* some more text" and return a sequence of TextSpans: italic<"some text">::rest
let (|Emphasized|_|) (ctx:ParsingContext) span =
  
  // Emphasis is not valid across multiple lines. this function checks for it
  let check (body: char list, length, empType, empLength, rest) =
    if String.containsNewLine body then
        None
    else
        Some(body, length, empType, empLength, rest)

  /// ***Text*** will be Strong( Italic... as first matching for Strong  
  match span with
  | ('_' :: tail) as input -> // Underline
    match input with
    | DelimitedText ['_'] (body, rest) ->
      check(body, body.Length + 2, Underline, 1, rest)
    | _ -> None
  | ('*' :: '*' :: tail) as input -> // Bold
    match input with
    | DelimitedText ['*'; '*'] (body, rest) -> 
      check(body, body.Length + 4, Bold, 2, rest)
    | _ -> None
  | ('*' :: tail) as input -> // Italic
    match input with
    | DelimitedText ['*'] (body, rest) -> 
      check(body, body.Length + 2, Italic, 1, rest)
    | _ -> None
  | _ -> None

/// recognizes notes which start with "[[" and end with "]]"
let (|Note|_|) input =
  // replace double space with new line
  let rec transform chars = 
      let newline = Environment.NewLine.ToCharArray() |> List.ofArray
      let pattern = NewLine(1) + "  " + NewLine(1) // new line pattern
      match chars with
      | List.StartsWithString pattern tail -> List.concat [ newline; newline ; (transform tail) ]
      | head :: tail -> head :: (transform tail)
      | [] -> []

  match input with
  | DelimitedWith ['['; '['] [']';']'] (body, rest) ->
      let x = String(body |> Array.ofList)
      Some ({ Text = body; Length = body.Length + 4; Offset = 2}, rest)
  | _ -> None

/// Parses a body of a block and recognizes all inline tags.
/// returns a sequence of FountainSpan
let rec parseChars (ctx:ParsingContext) acc numOfEscapedChars input = seq {

  // Zero or one literals, depending whether there is some accumulated input
  // 2015.01.07 Bryan Costanich - change Lazy.Create to Lazy<char []> because of some ambiguation err when building as a PCL
  let accLiterals = Lazy<char []>.Create(fun () ->
    if List.isEmpty acc then [] 
    else
        let text = String(List.rev acc |> Array.ofList)
        //let finalText = text
        let finalText = if ctx.PreserveIndenting then text else text.TrimStart()
        // add the number of escaped characters to the Range!
        [Literal(finalText, new Range(ctx.Position + text.IndexOf(finalText), finalText.Length + numOfEscapedChars))] )
  
  let patternForcedLineBreak = "  " + NewLine(1) // new line pattern

  match input with 
  | List.StartsWithString patternForcedLineBreak rest when ctx.TreatDoubleSpaceAsNewLine ->
  // ' ' :: ' ' :: '\r' :: '\n' :: rest when ctx.TreatDoubleSpaceAsNewLine ->
    yield HardLineBreak(new Range(ctx.Position, patternForcedLineBreak.Length))
    yield! parseChars (ctx.Offset(patternForcedLineBreak.Length)) [] numOfEscapedChars rest

  // markdown requires two spaces and then \r or \n, but fountain 
  // recognizes without
  // Recognizes explicit line-break at the end of line
  | '\r'::'\n'::rest
  | ('\n' | '\r')::rest ->
    //System.Diagnostics.Debug.WriteLine("found a hardlinebreak")
    yield! accLiterals.Value
    yield HardLineBreak(new Range(ctx.Position + acc.Length, Environment.NewLine.Length))
    yield! parseChars (ctx.Offset(acc.Length + Environment.NewLine.Length)) [] numOfEscapedChars rest

  // Ignore escaped characters that might mean something else
  | EscapedChar(c, rest) ->
      yield! parseChars ctx (c::acc) (numOfEscapedChars + 1) rest

  // Handle emphasised text
  | Emphasized ctx (body, length, emphasis, emphasisLength, rest) ->
      yield! accLiterals.Value
      let bodyParsed = parseChars (ctx.Offset(acc.Length + emphasisLength)) [] numOfEscapedChars body |> List.ofSeq
      yield emphasis(bodyParsed, (new Range(ctx.Position + acc.Length, length)))
      yield! parseChars (ctx.Offset(length + acc.Length)) [] numOfEscapedChars rest

  // Notes
  | Note (result, rest) ->
      yield! accLiterals.Value
      let body = parseChars (ctx.Offset(acc.Length + result.Offset).WithDoubleSpaceAsNewLine(true)) [] numOfEscapedChars result.Text |> List.ofSeq
      yield Note(body, new Range(ctx.Position + acc.Length, result.Length))
      yield! parseChars (ctx.Offset(acc.Length + result.Length)) [] numOfEscapedChars rest

  // This calls itself recursively on the rest of the list
  | x::xs -> 
      yield! parseChars ctx (x::acc) numOfEscapedChars xs 
  | [] ->
      yield! accLiterals.Value }

/// Parse body of a block into a list of Markdown inline spans
// trimming off \r\n?
let parseSpans (ctx:ParsingContext) (s:string) = 
  // why List.ofArray |> List.ofSeq?
  parseChars ctx [] 0 (s.ToCharArray() |> List.ofArray) |> List.ofSeq

//======================================================================================
// Part 2: Block Formatting

/// Recognizes a Section (# Some section, ## another section), prefixed with '#'s
let (|Section|_|) input =
  match input with
  | String.StartsWithRepeated "#" (n, header) :: rest ->
      // input starts with n piece of '#' characters followed by header
      let text = header
      // Why would we need to deal with the following? There is nothing about this in the Fountain spec
//      let text = 
//        // Drop "##" at the end, but only when it is preceded by some whitespace
//        // (For example "## Hello F#" should be "Hello F#")
//        if header.EndsWith "#" then
//          let noHash = header.TrimEnd [| '#' |]
//          if noHash.Length > 0 && Char.IsWhiteSpace(noHash.Chars(noHash.Length - 1))
//          then noHash else header
//        else header
      let length = if rest.IsEmpty then header.Length + n else header.Length + n + NewLineLength
      Some(n, { Text = header; Length = length; Offset = n }, rest)
  | rest ->
      None

let (|Boneyard|_|) input =
    // recongizing lines inside of the boneyard
    // TODO: now /* and */ should stand alone in a line. if it's not appropriate, Boneyard should be implemented as a span (it would be weird though :))
    let rec addLines (acc : string list) accLength = 
        function 
        | [] -> None // no beginning or ending of comment found
        | String.StartsWith "/*" head:string :: tail ->
            addLines acc (accLength + 2 + NewLineLength) tail // beginning of comment
        | String.StartsWith "*/" head:string :: tail -> 
            // end of comment
            let newLinesLength = if tail.IsEmpty then 0 else NewLineLength
            Some(List.rev acc, accLength + 2 + newLinesLength, tail)
        | [head] ->
            addLines (head :: acc) (accLength + head.Length) []
        | head :: tail ->
            addLines (head :: acc) (accLength + head.Length + NewLineLength) tail // inside or outside of comment
    match input with
    | String.StartsWith "/*" head:string :: tail ->
      match addLines [] 0 input with
      | Some([], _, rest) -> None // no comment found
      | Some(body, length, rest) -> Some({ Text = body; Length = length; Offset = 0 }, rest)
      | _ -> None
    | _ -> None

let (|SceneHeading|_|) (ctx:ParsingContext) (input:string list) =
  let parseSceneHeading (first:string) (tail:string list) offset =
    let head = first.TrimStart()
    let length = if tail.IsEmpty then NewLineLength else NewLineLength * 2 // check whether there is an empty line after the scene heading text
    match head with
    // look for normal heading
    | String.StartsWithAnyCaseInsensitive [ "INT./EXT"; "INT/EXT"; "I/E"; "内景/外景"; "屋内/屋外"; "内/外"; "INT"; "内景"; "屋内"; "内"; "EXT"; "外景"; "屋外"; "外"; "EST" ] matching ->
      match matching with 
      | String.StartsWithAny [ "."; " "] matching ->
        let result = {
          Text = head;
          Length = first.Length + length + offset;
          Offset = first.IndexOf(head) + offset
        }
        Some(false, result, tail)
      | _-> None
    // look for forced heading
    | String.StartsWith "." matching ->
      match matching with
      | String.StartsWith "." _ -> None
      | _->
        let recognition = {
            Text = head.Substring(1).Trim();
            Length = first.Length + length + offset;
            Offset = first.IndexOf(head) + 1 + offset 
          }
        Some(true, recognition, tail)
    | _ -> None

  match ctx.LastParsedBlock with
  | BlockWithTrailingEmptyLine x ->
      match input with
      | [] -> None
      | first :: EmptyLine :: rest ->
        parseSceneHeading first rest 0
      | _ -> None
  | _ ->
      match input with
      | [] -> None
      | EmptyLine :: first :: EmptyLine :: rest ->
        parseSceneHeading first rest NewLineLength
      | _ -> None


let (|Character|_|) (ctx:ParsingContext) (list:string list) =
  let parseCharacter (first:string) (rest:string list) lengthInc offsetInc =
        // length of possible Character block: \r\n<first>\r\n
        let length = if rest.IsEmpty then first.Length + lengthInc else first.Length + NewLineLength + lengthInc
        // trim white spaces as Character ignores indenting
        let head = first.TrimStart()
        // Character has to be preceded by empty line
        if (head.Length = 0) then
            None
        // matches "@McAVOY"
        else if (head.StartsWith "@") then
          let result = { Text = head.Substring(1); Length = length; Offset = 1 + offsetInc }
          if head.EndsWith(" ^") then
            Some(true, false, result, rest)
          else
            Some(true, true, result, rest)
        // matches "BOB" or "BOB JOHNSON" or "BOB (on the radio)" or "R2D2" but not "25D2"
        else
          let pattern = @"^\p{Lu}[\p{Lu}\d\s]*(\(.*\))?(\s+\^)?$"
          let m = Regex.Match(head, pattern)
          if m.Value = head then
            if m.Groups.Count > 1 && String.IsNullOrEmpty(m.Groups.[1].Value) = false then
              // check parenthetical extension for lowercase or uppercase
              // TODO: Do we really need to do this? The specification is not crystal clear about this.
              // If the extension can consist of mixed letters, than this block can be discarded
              let extension = m.Groups.[1].Value.ToCharArray() |> Seq.where(fun c -> Char.IsLetter(c))
              let allUpper = extension |> Seq.forall(fun c -> Char.IsUpper(c)) // all uppercase
              let allLower = extension |> Seq.forall(fun c -> Char.IsLower(c)) // all lowercase
              if allUpper || allLower then
                let result = { Text = head; Length = length; Offset = offsetInc }
                if m.Value.EndsWith("^") then
                  Some(false, false, { Text = m.Value.Remove(m.Value.Length - 1).Trim(); Length = length; Offset = first.IndexOf(head) + offsetInc }, rest)
                else
                  Some(false, true, { Text = head; Length = length; Offset = first.IndexOf(head) + offsetInc }, rest)
              else
                None
            else // no parenthetical extension found
              if m.Value.EndsWith("^") then
                // character for dual dialogue
                Some(false, false, { Text = m.Value.Remove(m.Value.Length - 1).Trim(); Length = length; Offset = first.IndexOf(head) + offsetInc }, rest)
              else
                Some(false, true, { Text = head; Length = length; Offset = first.IndexOf(head) + offsetInc }, rest)
          // does not match Character rules
          else
            None

  match ctx.LastParsedBlock with
  | BlockWithTrailingEmptyLine x ->
      match list with
      | [] -> None
      | first :: rest ->
        parseCharacter first rest 0 0
  | _ ->
      match list with
      | [] -> None
      | EmptyLine :: first :: rest ->
        parseCharacter first rest NewLineLength NewLineLength
      | _ -> None

/// Recognizes a PageBreak (3 or more consecutive equals and nothign more)
let (|PageBreak|_|) input = //function
  match input with
  | String.StartsWithRepeated "=" result :: rest ->
    // result: (number of repetitions, remaining string)
    if (fst result) >= 3 then
      let text = snd result
      let newLineCount = if rest.IsEmpty then 0 else 1
      match text.Trim() with
      | "" -> //after the trim, there should be nothing left.
         Some({ Text = null; Length = fst result + text.Length + NewLineLength * newLineCount; Offset = 0 }, rest)
      | _ -> 
         None
    else None
  | rest ->
     None

/// Recognizes a synposes (prefixed with `=` sign)
let (|Synopses|_|) = function
  | String.StartsWith "=" text :: rest ->
     let textTrimmed = text.Trim()
     let newLineCount = if rest.IsEmpty then 0 else 1
     Some({ Text = textTrimmed; Length = text.Length + 1 + NewLineLength * newLineCount; Offset = 1 + text.IndexOf(textTrimmed) }, rest)
  | rest ->
     None

/// Recognizes a Lyric (prefixed with ~)
let (|Lyrics|_|) = function
  | String.StartsWith "~" lyric:string :: rest ->
      let text = if rest.IsEmpty then lyric else lyric + NewLine(1)
      Some({ Text = text; Length = text.Length + 1; Offset = 1 }, rest)
  | rest ->
      None

/// Recognizes centered text (> The End <)
let (|Centered|_|) (input:string list) =
  match input with
  | head :: rest ->
    // Centered ignores indenting
    let text = head.Trim()
    if text.StartsWith(">") && text.EndsWith("<") then
        let textResult = text.Substring(1, text.Length - 2).Trim()
        let result = { 
            Text = textResult; 
            Length = if rest.IsEmpty then head.Length else head.Length + NewLineLength; 
            Offset = head.IndexOf(textResult) 
        }
        Some(result, rest) // strip '>' and '<'
    else
        None
  | _ -> None 

let isForcedAction (input:string) =
    input.StartsWith("!")

// Parenthetical
let (|Parenthetical|_|) (ctx:ParsingContext) (input:string list) =
  match ctx.LastParsedBlock with
  // parenthetical can come after character OR dialogue
  | Some (FountainSharp.Character(_)) 
  | Some (FountainSharp.Parenthetical(_)) // parenthetical can occur in a dialog which preceded by a character-parenthetical duo (in this case parenthetical is the last parsed block)
  | Some (FountainSharp.Dialogue(_)) ->
     match input with
     | blockContent :: rest ->
        let trimmed = blockContent.Trim()
        if (trimmed.StartsWith "(" && trimmed.EndsWith ")") then
            let body = trimmed.TrimStart('(').TrimEnd(')')
            let newLineCount = if rest.IsEmpty then 0 else 1
            Some({ Text = body; Length = blockContent.Length + NewLineLength * newLineCount; Offset = blockContent.IndexOf(body) }, rest)
        else
            None
     | [] -> None
  | _ -> None

//==== Transition

/// Matches a Transition block. Returns forced, MatchResult, rest
let (|Transition|_|) (ctx:ParsingContext) (input:string list) =
  // parsing Transition block from the line containing the text
  let parseTransition (head:string) rest offset =
    match [head] with
    | Centered _ -> None // prevent Centered being recognized as forced Transition
    | _ ->
        let blockContent = head.TrimStart() // Transition ignores indenting
        if blockContent.StartsWith "!" then // guard against forced action
            None
        elif blockContent.StartsWith ">" then // forced transition
            let text = blockContent.Substring(1).TrimStart()
            Some(true, { Text = text; Length = head.Length + NewLineLength * 2 + offset; Offset = head.IndexOf(text) + NewLineLength }, rest)
        elif blockContent.Equals "CUT TO BLACK." || blockContent.Equals "FADE TO BLACK." || blockContent.Equals "FADE OUT." then // non-forced transition
            // check for all uppercase
            if blockContent.ToCharArray() |> Seq.exists (fun c -> Char.IsLower(c)) then
              None
            else
              let text = blockContent.TrimStart()
              Some(false, { Text = text; Length = head.Length + NewLineLength * 2 + offset; Offset = head.IndexOf(text) + offset },  rest)
        elif blockContent.EndsWith(":") then
            // check for all uppercase
            if blockContent.ToCharArray() |> Seq.exists (fun c -> Char.IsLower(c)) then
              None
            else
              let text = blockContent.TrimStart()
              Some(false, { Text = text; Length = head.Length + NewLineLength * 2 + offset; Offset = head.IndexOf(text) + offset },  rest)
        else
            None

  // Has to be preceded by and followed by an empty line
  match ctx.LastParsedBlock with
  | BlockWithTrailingEmptyLine x ->
      match input with
      | first :: EmptyLine :: rest ->
        parseTransition first rest 0
      | _ -> None
  | _ ->
      match input with
      | [] -> None
      | EmptyLine :: first :: EmptyLine :: rest ->
        parseTransition first rest NewLineLength
      | _ -> None


//==== Dialogue

let (|Dialogue|_|) (ctx:ParsingContext) (input:string list) =
  match ctx.LastParsedBlock with
  | Some (FountainSharp.Character(_)) 
  | Some (FountainSharp.Parenthetical(_)) ->
     // Dialogue starts after Character or Character (parenthetical)
     // look ahead and keep matching while it's none of these.
     match List.partitionWhileLookahead (function
     | SceneHeading ctx _ -> false //note: it's decomposing the match and the rest and discarding the rest: `SceneHeading _` 
     | Character ctx _ -> false
     | Lyrics _ -> false
     | Transition ctx _ -> false // ugh. need to pass the last parsed block.
     | Centered _ -> false
     | Section _ -> false
     | Synopses _ -> false
     | PageBreak _ -> false
     | Parenthetical ctx _ -> false
     | Boneyard _ -> false
     | _ -> true) input with // if we found a match, and it's not empty, return the Action and the rest
        | [], _ -> None
        | matching, rest ->
          // parsing dialogue's lines
          let rec addLines (acc: string list) (accLength : int) = function
            // TODO: the following matches could be simpler, I think
            | first :: second :: tail as input ->
                // add to length if the Dialogue block is followed by any line
                let lengthInc = if tail.IsEmpty && rest.IsEmpty then 0 else NewLineLength
                if first = EmptyLine then
                    if second.StartsWith("  ") then // dialogue continues
                        addLines (second :: first :: acc) (accLength + first.Length + NewLineLength + second.Length + lengthInc) tail
                    else
                        Some(List.rev acc, accLength, List.append(first :: second :: tail) rest)
                elif isForcedAction(first) then // stop at forced Action
                    Some(List.rev acc, accLength, List.append input rest)
                else if second = EmptyLine then
                    addLines (first :: acc) (accLength + first.Length + NewLineLength) (second :: tail)
                else
                    addLines (second :: first :: acc) (accLength + first.Length + NewLineLength + second.Length + lengthInc) tail
            | [head] ->
                if isForcedAction(head) then // stop at forced Action
                    Some(List.rev acc, accLength, rest)
                else
                    // add to length if the Dialogue block is followed by any line
                    let lengthInc = if rest.IsEmpty then 0 else NewLineLength
                    addLines (head :: acc) (accLength + head.Length + lengthInc) []
            | [] ->
                if accLength = 0 then
                  None
                else
                  Some(List.rev acc, accLength, rest) // traversed all the lines

          let lines = addLines [] 0 matching
          match lines with
          | Some([], length, rest) ->
            match rest with
            | EmptyLine :: tail ->
              // starts with empty line (this is treated as dialogue)
              Some({ Text = Environment.NewLine; Length = NewLineLength; Offset = 0 }, tail)
            | _ -> None  // no lines found
          | Some(body, length, rest) ->
              //let body = body |> List.map(fun line -> line.Trim())
              let result = String.asSingleString(body, Environment.NewLine, false)
              Some({ Text = result; Length = length; Offset = 0 }, rest)
          | _ -> None
  | _ -> None

//==== /Dialogue


//==== Dual Dialogue

let (|DualDialogue|_|) (ctx: ParsingContext) (input:string list) =
  // parse input for Character or Character, Parenthetical and return the list of them  
  let rec parseCharacter (ctx:ParsingContext, input:string list, acc) = 
    match input with
    | Character ctx (forced, primary, result, rest) as item -> 
        let characterItem = Character(forced, primary, parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
        let ctx = ctx.Offset(result.Length).WithLastParsedBlock(Some(characterItem))
        match rest with
        | Parenthetical ctx (result, rest) ->
            let parentheticalItem = Parenthetical(parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
            parseCharacter (ctx.Offset(result.Length).WithLastParsedBlock(Some(parentheticalItem)), rest, parentheticalItem :: characterItem :: acc)
        | _ ->
            parseCharacter (ctx, rest, characterItem :: acc)
    | _ -> if acc.Length = 0 then None else Some(ctx, acc, input)

  // parse input for Dialogue or Dialogue, Parenthetical and return the list of them  
  let rec parseDialogue (ctx:ParsingContext, input:string list, acc) = 
    match input with
    | Dialogue ctx (dialogResult, rest) -> 
        let dialogueItem = Dialogue(parseSpans ctx dialogResult.Text, new Range(ctx.Position, dialogResult.Length))
        let ctx = ctx.Offset(dialogResult.Length).WithLastParsedBlock(Some(dialogueItem))
        match rest with
        | Parenthetical ctx (result, rest) ->
            let parentheticalItem = Parenthetical(parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
            parseDialogue (ctx.Offset(result.Length).WithLastParsedBlock(Some(parentheticalItem)), rest, parentheticalItem :: dialogueItem :: acc)
        | _ ->
            parseDialogue (ctx, rest, dialogueItem :: acc)
    | _ -> if acc.Length = 0 then None else Some(ctx, acc, input)

  // parse input for (Character, Dialogue) pairs and return the list of them  
  let rec parse (ctx : ParsingContext, input:string list, acc) = 
    if input.Length = 0 then
        Some(List.rev acc, input)
    else
      match parseCharacter(ctx, input, []) with
      | Some(ctx, characterBlocks, rest) -> 
          match parseDialogue(ctx, rest, []) with
          | Some(ctx, dialogueBlocks, rest) ->
              parse (ctx, rest, List.concat [dialogueBlocks; characterBlocks; acc])
          | _ -> Some(List.rev acc, input)
      | _ -> Some(List.rev acc, input)

  let isSecondary block =
    match block with
    | FountainBlockElement.Character(forced, primary, spans, r) ->
      not primary
    | _ -> false

  let isPrimary block = 
    match block with
    | FountainBlockElement.Character(forced, primary, spans, r) ->
      primary
    | _ -> false

  // at least 2 (Character, Dialogue) blocks have to be found and at least one of them should be secondary character (marked by a caret)
    
  match parse (ctx, input, []) with
  | Some([], _) -> None // no (Character, Dialogue) blocks found
  | Some(list, rest) ->
    let length = list |> List.map(fun block -> block.Range.Length) |> List.sum
    if List.tryFind isPrimary list <> None &&  List.tryFind isSecondary list <> None then
       Some(list, length, rest)
    else
       None
  | None -> None

//==== /Dual Dialogue

//==== Action

let (|Action|_|) (ctx:ParsingContext) input =
  let ctxForLookAhead = ctx.WithLastParsedBlock(Some(FountainSharp.Action(false, [], Range.empty)))
  // look ahead and keep matching while it's none of these.
  match List.partitionWhileLookahead (function
    | SceneHeading ctxForLookAhead _ -> false //note: it's decomposing the match and the rest and discarding the rest: `SceneHeading _` 
    | Character ctxForLookAhead _ -> false
    | Lyrics _ -> false
    | Transition ctxForLookAhead _ -> false // ugh. need to pass the last parsed block.
    | Centered _ -> false
    | Section _ -> false
    | Synopses _ -> false
    | PageBreak _ -> false
    | Boneyard _ -> false
    | _ -> true) input with // if we found a match, and it's not empty, return the Action and the rest
      | matching, rest ->
        match matching with
        | [] -> None
        | [EmptyLine] ->
            Some(false, { Text = NewLine(1); Length = NewLineLength; Offset = 0 }, rest)
        | hd :: tail ->
          let s = String.asSingleString(hd :: tail, Environment.NewLine, not rest.IsEmpty)
          let length = if rest.IsEmpty then s.Length else s.Length + NewLineLength
          if (hd.StartsWith "!") then // forced Action, trim off the '!'
            Some(true, { Text = s.Substring(1); Length = s.Length; Offset = 1 }, rest)
          else
            Some(false, { Text = s; Length = s.Length; Offset = 0 }, rest)
      //| _ -> None

//==== /Action

//==== TitlePage

let (|TitlePage|_|) (ctx:ParsingContext) (input: string list) =
  match ctx.LastParsedBlock with
  | Some(_) -> None // Title page must be the first block of the document 
  | None ->
    // match "key: value" pattern at the beginning of the input as far as it is possible, and returns the matching values as well the remaining input
    let rec matchAndRemove acc accLength (input:string) =
      let validCharacterClass = "[^:]"
      let pattern = String.Format(@"^(?<key>\b{0}+):(?<value>{0}+\n)", validCharacterClass)
      let m = Regex.Match(input, pattern, RegexOptions.Singleline)
      if m.Success = false then
        (List.rev acc, input) // no more match found
      else
       let keyGroup = m.Groups.["key"] // text before ':'
       let valueGroup = m.Groups.["value"] // text after ':'
       // let's parse spans - white spaces must be trimmed per line
       let spans = valueGroup.Value.Split([|Environment.NewLine|], StringSplitOptions.None) |> List.ofArray |> String.concat Environment.NewLine |> parseSpans (ctx.Offset(accLength + valueGroup.Index).WithPreserveIndenting(true))
       let keyBlock = (keyGroup.Value, new Range(ctx.Position + accLength + keyGroup.Index, keyGroup.Value.Length + 1))
       matchAndRemove ((keyBlock, spans) :: acc) (accLength + m.Length) (input.Remove(m.Index, m.Length))
    
    // TODO: spare conversion from list to string and back to list of the remaining text!
    let inputAsSingleString = String.asSingleString(input, NewLine(1), false) // treat input as one string
    // when an empty line is present, only assume Title Page until that
    let indexOfEmptyLine = inputAsSingleString.IndexOf(NewLine(2))
    let titlePageText = if indexOfEmptyLine = -1 then inputAsSingleString else inputAsSingleString.Substring(0, indexOfEmptyLine + Environment.NewLine.Length)
    match matchAndRemove [] 0 titlePageText with
    | ([], _) -> None
    | (keyValuePairs, rest) ->
        Some(keyValuePairs, titlePageText.Length + Environment.NewLine.Length, String.asStringList(inputAsSingleString.Substring(indexOfEmptyLine + Environment.NewLine.Length * 2), NewLine(1)))

//==== /TitlePage

/// Parse a list of lines into a sequence of fountain blocks
/// note, we pass the lastParsedBlock because some blocks are dependent on what came before. dialogue, for 
/// instance, comes after Character
/// 
let rec parseBlocks (ctx:ParsingContext) (lines: _ list) = seq {
  // NOTE: Order of matching is important here. for instance, if you matched dialogue before 
  // parenthetical, you'd never get parenthetical

  match lines with
  | TitlePage ctx (keyValuePairs, length, rest) ->
     let item = TitlePage(keyValuePairs, new Range(0, length))
     yield item
     yield! parseBlocks (ctx.Offset(length).WithLastParsedBlock(Some(item))) rest
  
  | Boneyard(result, rest) ->
     let item = Boneyard(String.asSingleString(result.Text, Environment.NewLine, false), new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  // Recognize remaining types of blocks/paragraphs
  
  | SceneHeading ctx (forced, result, rest) ->
     let item = SceneHeading(forced, parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | Section(n, result, rest) ->
     let body = parseSpans (ctx.Offset(result.Offset)) result.Text
     let item = Section(n, body, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | DualDialogue ctx (blocks, length, rest) ->
     let item = DualDialogue(blocks, new Range(ctx.Position, length))
     yield item
     yield! parseBlocks (ctx.Offset(length).WithLastParsedBlock(Some(item))) rest
  
  | Character ctx (forced, primary, result, rest) ->
     let item = Character(forced, primary, parseSpans (ctx.Offset(result.Offset).WithPreserveIndenting(true)) result.Text, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | PageBreak(result, rest) ->
     let item = PageBreak(new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | Synopses(result, rest) ->
     let item = Synopses(parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | Lyrics(result, rest) ->
     let body = parseSpans (ctx.Offset(result.Offset)) result.Text
     let item = Lyrics(body, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest

  | Centered(result, rest) ->
     let body = parseSpans (ctx.Offset(result.Offset)) result.Text
     let item = Centered(body, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest

  | Transition ctx (forced, result, rest) ->
     let spans = parseSpans (ctx.Offset(result.Offset)) result.Text
     let item = Transition(forced, spans, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest
  
  | Parenthetical ctx (result, rest) ->
     let item = Parenthetical(parseSpans (ctx.Offset(result.Offset)) result.Text, new Range(ctx.Position, result.Length))
     yield item
     yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest

  | Dialogue ctx (result, rest) ->
    let spans = parseSpans (ctx.WithDoubleSpaceAsNewLine(true)) result.Text
    let item = Dialogue(spans, new Range(ctx.Position, result.Length))
    yield item
    yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest // go on to parse the rest

  | Action ctx (forced, result, rest) ->
    // body: as a single string. this can be parsed for spans much better
    let spans = parseSpans (ctx.WithPreserveIndenting(true).Offset(result.Offset)) result.Text
    let item = Action(forced, spans, new Range(ctx.Position, result.Length))
    yield item
    yield! parseBlocks (ctx.Offset(result.Length).WithLastParsedBlock(Some(item))) rest // go on to parse the rest
  | first :: rest -> 
    yield! parseBlocks ctx rest // let's try to skip this line and recognize the rest
  | _ -> () // reached the end
  }

  //| _ -> failwithf "Unexpectedly stopped!\n%A" lines }
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FountainSharp;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System.Diagnostics;
using System.Threading;

namespace GameStoryEdit
{
    [DebuggerDisplay("Scene_{SceneBlocks}")]
    public class FountainGame
    {
        public FountainDocument Fountain { get; }
        public Blocks Blocks => new Blocks(Fountain.Blocks);
        public FSharpList<FountainBlockElement> FSharpBlocks => Fountain.Blocks;
        public List<SceneBlock> SceneBlocks => Blocks.GetSceneBlocks();
        public List<DialogueBlock> DialogueBlocks => Blocks.GetDialogueBlocks();
        public string GetText(Range range) => Fountain.GetText(range);
        public void ReplaceText(int location, int length, string replaceText) => Fountain.ReplaceText(location, length, replaceText);
        public string Html => HtmlFormatter.WriteHtml(Fountain);
        public FountainGame(string Text) => Fountain = FountainDocument.Parse(Text);
        private FountainGame(FountainDocument fountainDocument) => Fountain = fountainDocument;
        public static async Task<FountainGame> GetValueAsync(string Text) => new FountainGame(await FountainDocument.ParseAsync(Text));
    }

    public class Blocks
    {
        public List<Action> Actions { get; }
        public List<Boneyard> Boneyards { get; }
        public List<Centered> Centereds { get; }
        public List<Character> Characters { get; }
        public List<Dialogue> Dialogues { get; }
        public List<DualDialogue> DualDialogues { get; }
        public List<Lyrics> Lyrics { get; }
        public List<PageBreak> PageBreaks { get; }
        public List<Parenthetical> Parentheticals { get; }
        public List<SceneHeading> SceneHeadings { get; }
        public List<Section> Sections { get; }
        public List<Synopses> Synopses { get; }
        public List<TitlePage> TitlePages { get; }
        public List<Transition> Transitions { get; }
        public FSharpList<FountainBlockElement> FSharpBlocks { get; }
        private List<int> SceneHeading_Index = new List<int>();
        private List<int> Character_Index = new List<int>();
        private List<int> Dialogue_Index = new List<int>();
        public List<SceneBlock> GetSceneBlocks()
        {
            List<SceneBlock> sceneBlocks = new List<SceneBlock>();
            List<int> Index = SceneHeading_Index;
            Index.Add(FSharpBlocks.Count());
            for (int i = 0; i < Index.Count - 1; i++)
                sceneBlocks.Add(new SceneBlock(FSharpBlocks.GetSlice(Index[i], Index[i + 1] - 1)));
            return sceneBlocks;
        }
        public List<DialogueBlock> GetDialogueBlocks()
        {
            List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();
            for (int i = 0; i < Character_Index.Count; i++)
                if (Character_Index.Count() == Dialogue_Index.Count()) dialogueBlocks.Add(new DialogueBlock(FSharpBlocks.GetSlice(Character_Index[i], Dialogue_Index[i])));
                else
                {
                    if (i == Character_Index.Count - 1)
                    {
                        List<int> Index = Dialogue_Index.Where(di => di > Character_Index[i] && di < FSharpBlocks.Count()).ToList();
                        dialogueBlocks.Add(new DialogueBlock(FSharpBlocks.GetSlice(Character_Index[i], Index[Index.Count() - 1])));
                    }
                    else
                    {
                        List<int> Index = Dialogue_Index.Where(di => di > Character_Index[i] && di < Character_Index[i + 1]).ToList();
                        dialogueBlocks.Add(new DialogueBlock(FSharpBlocks.GetSlice(Character_Index[i], Index[Index.Count()-1])));
                    }
                }
            return dialogueBlocks;
        }
        public Blocks(FSharpList<FountainBlockElement> blocks)
        {
            Actions = new List<Action>();
            Boneyards = new List<Boneyard>();
            Centereds = new List<Centered>();
            Characters = new List<Character>();
            Dialogues = new List<Dialogue>();
            DualDialogues = new List<DualDialogue>();
            Lyrics = new List<Lyrics>();
            PageBreaks = new List<PageBreak>();
            Parentheticals = new List<Parenthetical>();
            SceneHeadings = new List<SceneHeading>();
            Sections = new List<Section>();
            Synopses = new List<Synopses>();
            TitlePages = new List<TitlePage>();
            Transitions = new List<Transition>();
            FSharpBlocks = blocks;
            for (int i = 0; i < blocks.Count(); i++)
            {
                FountainBlockElement b = blocks[i];
                if (b.IsAction) Actions.Add(new Action(b));
                if (b.IsBoneyard) Boneyards.Add(new Boneyard(b));
                if (b.IsCentered) Centereds.Add(new Centered(b));
                if (b.IsCharacter) { Characters.Add(new Character(b)); Character_Index.Add(i); }
                if (b.IsDialogue) { Dialogues.Add(new Dialogue(b)); Dialogue_Index.Add(i); }
                if (b.IsDualDialogue)
                {
                    DualDialogue d = new DualDialogue(b);
                    DualDialogues.Add(d);
                    d.Blocks.Characters.ForEach(c => Characters.Add(c));
                    d.Blocks.Dialogues.ForEach(c => Dialogues.Add(c));
                }
                if (b.IsLyrics) Lyrics.Add(new Lyrics(b));
                if (b.IsPageBreak) PageBreaks.Add(new PageBreak(b));
                if (b.IsParenthetical) Parentheticals.Add(new Parenthetical(b));
                if (b.IsSceneHeading) { SceneHeadings.Add(new SceneHeading(b)); SceneHeading_Index.Add(i); }
                if (b.IsSection) Sections.Add(new Section(b));
                if (b.IsSynopses) Synopses.Add(new Synopses(b));
                if (b.IsTitlePage) TitlePages.Add(new TitlePage(b));
                if (b.IsTransition) Transitions.Add(new Transition(b));
            }
        }
    }
    [DebuggerDisplay("{Range}")]
    public class SceneBlock : Blocks
    {
        public List<DialogueBlock> DialogueBlocks => GetDialogueBlocks();
        public Range Range { get; }
        public SceneBlock(FSharpList<FountainBlockElement> blocks) : base(blocks)
        {
            Range = new Range(blocks[0].Range.Location, blocks[blocks.Length - 1].Range.EndLocation - blocks[0].Range.Location + 1);
        }
    }
    [DebuggerDisplay("{Range}")]
    public class DialogueBlock : Blocks
    {
        public Range Range { get; }
        public DialogueBlock(FSharpList<FountainBlockElement> blocks) : base(blocks)
        {
            Range = new Range(blocks[0].Range.Location, blocks[blocks.Length - 1].Range.EndLocation - blocks[0].Range.Location + 1);
        }
    }

    [DebuggerDisplay("{FSharpAction}")]
    public class Action
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Action FSharpAction { get; }
        public Action(FountainBlockElement Action)
        {
            FSharpAction = (FountainBlockElement.Action)Action;
            Forced = FSharpAction.Item1;
            Spans = new Spans(FSharpAction.Item2);
            Range = FSharpAction.Item3;
        }
    }
    [DebuggerDisplay("{FSharpBoneyard}")]
    public class Boneyard
    {
        public string Text { get; }
        public Range Range { get; }
        public FountainBlockElement.Boneyard FSharpBoneyard { get; }
        public Boneyard(FountainBlockElement Boneyard)
        {
            FSharpBoneyard = (FountainBlockElement.Boneyard)Boneyard;
            Text = FSharpBoneyard.Item1;
            Range = FSharpBoneyard.Item2;
        }
    }
    [DebuggerDisplay("{FSharpCentered}")]
    public class Centered
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Centered FSharpCentered { get; }
        public Centered(FountainBlockElement Centered)
        {
            FSharpCentered = (FountainBlockElement.Centered)Centered;
            Spans = new Spans(FSharpCentered.Item1);
            Range = FSharpCentered.Item2;
        }
    }
    [DebuggerDisplay("{FSharpCharacter}")]
    public class Character
    {
        public bool Forced { get; }
        public bool Primary { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Character FSharpCharacter { get; }
        public Character(FountainBlockElement Character)
        {
            FSharpCharacter = (FountainBlockElement.Character)Character;
            Forced = FSharpCharacter.Item1;
            Primary = FSharpCharacter.Item2;
            Spans = new Spans(FSharpCharacter.Item3);
            Range = FSharpCharacter.Item4;
        }
    }
    [DebuggerDisplay("{FSharpDialogue}")]
    public class Dialogue
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Dialogue FSharpDialogue { get; }
        public Dialogue(FountainBlockElement Dialogue)
        {
            FSharpDialogue = (FountainBlockElement.Dialogue)Dialogue;
            Spans = new Spans(FSharpDialogue.Item1);
            Range = FSharpDialogue.Item2;
        }
    }
    [DebuggerDisplay("{FSharpDualDialogue}")]
    public class DualDialogue
    {
        public Blocks Blocks { get; }
        public Range Range { get; }
        public FountainBlockElement.DualDialogue FSharpDualDialogue { get; }
        public DualDialogue(FountainBlockElement DualDialogue)
        {
            FSharpDualDialogue = (FountainBlockElement.DualDialogue)DualDialogue;
            Blocks = new Blocks(FSharpDualDialogue.Item1);
            Range = FSharpDualDialogue.Item2;
        }
    }
    [DebuggerDisplay("{FSharpLyrics}")]
    public class Lyrics
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Lyrics FSharpLyrics { get; }
        public Lyrics(FountainBlockElement Lyrics)
        {
            FSharpLyrics = (FountainBlockElement.Lyrics)Lyrics;
            Spans = new Spans(FSharpLyrics.Item1);
            Range = FSharpLyrics.Item2;
        }
    }
    [DebuggerDisplay("{FSharpPageBreak}")]
    public class PageBreak
    {
        public Range Range { get; }
        public FountainBlockElement.PageBreak FSharpPageBreak { get; }
        public PageBreak(FountainBlockElement PageBreak)
        {
            FSharpPageBreak = (FountainBlockElement.PageBreak)PageBreak;
            Range = FSharpPageBreak.Item;
        }
    }
    [DebuggerDisplay("{FSarpParenthetical}")]
    public class Parenthetical
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Parenthetical FSarpParenthetical { get; }
        public Parenthetical(FountainBlockElement Parenthetical)
        {
            FSarpParenthetical = (FountainBlockElement.Parenthetical)Parenthetical;
            Spans = new Spans(FSarpParenthetical.Item1);
            Range = FSarpParenthetical.Item2;
        }
    }
    [DebuggerDisplay("{FSharpSceneHeading}")]
    public class SceneHeading
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.SceneHeading FSharpSceneHeading { get; }
        public SceneHeading(FountainBlockElement SceneHeading)
        {
            FSharpSceneHeading = (FountainBlockElement.SceneHeading)SceneHeading;
            Forced = FSharpSceneHeading.Item1;
            Spans = new Spans(FSharpSceneHeading.Item2);
            Range = FSharpSceneHeading.Item3;
        }
    }
    [DebuggerDisplay("{FSharpSection}")]
    public class Section
    {
        public int N { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Section FSharpSection { get; }
        public Section(FountainBlockElement Section)
        {
            FSharpSection = (FountainBlockElement.Section)Section;
            N = FSharpSection.Item1;
            Spans = new Spans(FSharpSection.Item2);
            Range = FSharpSection.Item3;
        }
    }
    [DebuggerDisplay("{FSharpSynopses}")]
    public class Synopses
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Synopses FSharpSynopses { get; }
        public Synopses(FountainBlockElement Synopses)
        {
            FSharpSynopses = (FountainBlockElement.Synopses)Synopses;
            Spans = new Spans(FSharpSynopses.Item1);
            Range = FSharpSynopses.Item2;
        }
    }
    [DebuggerDisplay("{FSharpTitlePage}")]
    public class TitlePage
    {
        public List<KeyValuePair> KeyValuePairs { get; }
        public Range Range { get; }
        public FountainBlockElement.TitlePage FSharpTitlePage { get; }
        public TitlePage(FountainBlockElement TitlePage)
        {
            KeyValuePairs = new List<KeyValuePair>();
            FSharpTitlePage = (FountainBlockElement.TitlePage)TitlePage;
            foreach (Tuple<Tuple<string, Range>, FSharpList<FountainSpanElement>> k in FSharpTitlePage.Item1)
            {
                KeyValuePairs.Add(new KeyValuePair(k));
            }
            Range = FSharpTitlePage.Item2;
        }

        [DebuggerDisplay("{Key}")]
        public class KeyValuePair
        {
            public string Key { get; }
            public Range Range { get; }
            public Spans Spans { get; }
            public KeyValuePair(Tuple<Tuple<string, Range>, FSharpList<FountainSpanElement>> KeyValuePair)
            {
                KeyValuePair.Deconstruct(out Tuple<string, Range> key, out FSharpList<FountainSpanElement> spans);
                Key = key.Item1;
                Range = key.Item2;
                Spans = new Spans(spans);
            }
        }
    }
    [DebuggerDisplay("{FSharpTransition}")]
    public class Transition
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public FountainBlockElement.Transition FSharpTransition { get; }
        public Transition(FountainBlockElement Transition)
        {
            FSharpTransition = (FountainBlockElement.Transition)Transition;
            Forced = FSharpTransition.Item1;
            Spans = new Spans(FSharpTransition.Item2);
            Range = FSharpTransition.Item3;
        }
    }

    public class Spans
    {
        public List<Note> Notes { get; }
        public List<Literal> Literals { get; }
        public List<Bold> Bolds { get; }
        public List<Italic> Italics { get; }
        public List<Underline> Underlines { get; }
        public List<HardLineBreak> HardLineBreaks { get; }
        public Spans(FSharpList<FountainSpanElement> spans)
        {
            Notes = new List<Note>();
            Literals = new List<Literal>();
            Bolds = new List<Bold>();
            Italics = new List<Italic>();
            Underlines = new List<Underline>();
            HardLineBreaks = new List<HardLineBreak>();
            foreach (FountainSpanElement b in spans)
            {
                if (b.IsNote) Notes.Add(new Note(b));
                if (b.IsLiteral) Literals.Add(new Literal(b));
                if (b.IsBold) Bolds.Add(new Bold(b));
                if (b.IsItalic) Italics.Add(new Italic(b));
                if (b.IsUnderline) Underlines.Add(new Underline(b));
                if (b.IsHardLineBreak) HardLineBreaks.Add(new HardLineBreak(b));
            }
        }

        [DebuggerDisplay("{FSharpNote}")]
        public class Note
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public FountainSpanElement.Note FSharpNote { get; }
            public Note(FountainSpanElement Note)
            {
                FSharpNote = (FountainSpanElement.Note)Note;
                Spans = FSharpNote.Item1;
                Range = FSharpNote.Item2;
            }
        }
        [DebuggerDisplay("{FSharpLiteral}")]
        public class Literal
        {
            public string Text { get; }
            public Range Range { get; }
            public FountainSpanElement.Literal FSharpLiteral { get; }
            public Literal(FountainSpanElement Literal)
            {
                FSharpLiteral = (FountainSpanElement.Literal)Literal;
                Text = FSharpLiteral.Item1;
                Range = FSharpLiteral.Item2;
            }
        }
        [DebuggerDisplay("{FSharpBold}")]
        public class Bold
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public FountainSpanElement.Bold FSharpBold { get; }
            public Bold(FountainSpanElement Bold)
            {
                FSharpBold = (FountainSpanElement.Bold)Bold;
                Spans = FSharpBold.Item1;
                Range = FSharpBold.Item2;
            }
        }
        [DebuggerDisplay("{FSharpItalic}")]
        public class Italic
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public FountainSpanElement.Italic FSharpItalic { get; }
            public Italic(FountainSpanElement Italic)
            {
                FSharpItalic = (FountainSpanElement.Italic)Italic;
                Spans = FSharpItalic.Item1;
                Range = FSharpItalic.Item2;
            }
        }
        [DebuggerDisplay("{FSharpUnderline}")]
        public class Underline
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public FountainSpanElement.Underline FSharpUnderline { get; }
            public Underline(FountainSpanElement Underline)
            {
                FSharpUnderline = (FountainSpanElement.Underline)Underline;
                Spans = FSharpUnderline.Item1;
                Range = FSharpUnderline.Item2;
            }
        }
        [DebuggerDisplay("{FSharpHardLineBreak}")]
        public class HardLineBreak
        {
            public Range Range { get; }
            public FountainSpanElement.HardLineBreak FSharpHardLineBreak { get; }
            public HardLineBreak(FountainSpanElement HardLineBreak)
            {
                FSharpHardLineBreak = (FountainSpanElement.HardLineBreak)HardLineBreak;
                Range = FSharpHardLineBreak.Item;
            }
        }
    }
}

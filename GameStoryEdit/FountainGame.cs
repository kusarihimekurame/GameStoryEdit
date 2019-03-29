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

namespace GameStoryEdit
{
    class FountainGame
    {
        public FountainDocument Fountain { get; }
        public Blocks Blocks => new Blocks(Fountain.Blocks);
        public string Html => HtmlFormatter.WriteHtml(Fountain);
        public FountainGame(string Text)
        {
            Fountain = FountainDocument.Parse(Text);
        }
    }

    class Blocks
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
            foreach (FountainBlockElement b in blocks)
            {
                if (b.IsAction) Actions.Add(new Action(b));
                if (b.IsBoneyard) Boneyards.Add(new Boneyard(b));
                if (b.IsCentered) Centereds.Add(new Centered(b));
                if (b.IsCharacter) Characters.Add(new Character(b));
                if (b.IsDialogue) Dialogues.Add(new Dialogue(b));
                if (b.IsDualDialogue) DualDialogues.Add(new DualDialogue(b));
                if (b.IsLyrics) Lyrics.Add(new Lyrics(b));
                if (b.IsPageBreak) PageBreaks.Add(new PageBreak(b));
                if (b.IsParenthetical) Parentheticals.Add(new Parenthetical(b));
                if (b.IsSceneHeading) SceneHeadings.Add(new SceneHeading(b));
                if (b.IsSection) Sections.Add(new Section(b));
                if (b.IsSynopses) Synopses.Add(new Synopses(b));
                if (b.IsTitlePage) TitlePages.Add(new TitlePage(b));
                if (b.IsTransition) Transitions.Add(new Transition(b));
            }
        }
    }

    class Action
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public Action(FountainBlockElement Action)
        {
            FountainBlockElement.Action action = (FountainBlockElement.Action)Action;
            Forced = action.Item1;
            Spans = new Spans(action.Item2);
            Range = action.Item3;
        }
    }
    class Boneyard
    {
        public string Text { get; }
        public Range Range { get; }
        public Boneyard(FountainBlockElement Boneyard)
        {
            FountainBlockElement.Boneyard boneyard = (FountainBlockElement.Boneyard)Boneyard;
            Text = boneyard.Item1;
            Range = boneyard.Item2;
        }
    }
    class Centered
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public Centered(FountainBlockElement Centered)
        {
            FountainBlockElement.Centered centered = (FountainBlockElement.Centered)Centered;
            Spans = new Spans(centered.Item1);
            Range = centered.Item2;
        }
    }
    class Character
    {
        public bool Forced { get; }
        public bool Primary { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public Character(FountainBlockElement Character)
        {
            FountainBlockElement.Character character = (FountainBlockElement.Character)Character;
            Forced = character.Item1;
            Primary = character.Item2;
            Spans = new Spans(character.Item3);
            Range = character.Item4;
        }
    }
    class Dialogue
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public Dialogue(FountainBlockElement Dialogue)
        {
            FountainBlockElement.Dialogue dialogue = (FountainBlockElement.Dialogue)Dialogue;
            Spans = new Spans(dialogue.Item1);
            Range = dialogue.Item2;
        }
    }
    class DualDialogue
    {
        public Blocks Blocks { get; }
        public Range Range { get; }
        public DualDialogue(FountainBlockElement DualDialogue)
        {
            FountainBlockElement.DualDialogue dualDialogue = (FountainBlockElement.DualDialogue)DualDialogue;
            Blocks = new Blocks(dualDialogue.Item1);
            Range = dualDialogue.Item2;
        }
    }
    class Lyrics
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public Lyrics(FountainBlockElement Lyrics)
        {
            FountainBlockElement.Lyrics lyrics = (FountainBlockElement.Lyrics)Lyrics;
            Spans = new Spans(lyrics.Item1);
            Range = lyrics.Item2;
        }
    }
    class PageBreak
    {
        public Range Range { get; }
        public PageBreak(FountainBlockElement PageBreak)
        {
            FountainBlockElement.PageBreak pageBreak = (FountainBlockElement.PageBreak)PageBreak;
            Range = pageBreak.Item;
        }
    }
    class Parenthetical
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public Parenthetical(FountainBlockElement Parenthetical)
        {
            FountainBlockElement.Parenthetical parenthetical = (FountainBlockElement.Parenthetical)Parenthetical;
            Spans = new Spans(parenthetical.Item1);
            Range = parenthetical.Item2;
        }
    }
    class SceneHeading
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public SceneHeading(FountainBlockElement SceneHeading)
        {
            FountainBlockElement.SceneHeading sceneHeading = (FountainBlockElement.SceneHeading)SceneHeading;
            Forced = sceneHeading.Item1;
            Spans = new Spans(sceneHeading.Item2);
            Range = sceneHeading.Item3;
        }
    }
    class Section
    {
        public int N { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public Section(FountainBlockElement Section)
        {
            FountainBlockElement.Section section = (FountainBlockElement.Section)Section;
            N = section.Item1;
            Spans = new Spans(section.Item2);
            Range = section.Item3;
        }
    }
    class Synopses
    {
        public Spans Spans { get; }
        public Range Range { get; }
        public Synopses(FountainBlockElement Synopses)
        {
            FountainBlockElement.Synopses synopses = (FountainBlockElement.Synopses)Synopses;
            Spans = new Spans(synopses.Item1);
            Range = synopses.Item2;
        }
    }
    class TitlePage
    {
        public List<KeyValuePair> KeyValuePairs { get; }
        public Range Range { get; }
        public TitlePage(FountainBlockElement TitlePage)
        {
            KeyValuePairs = new List<KeyValuePair>();
            FountainBlockElement.TitlePage titlePage = (FountainBlockElement.TitlePage)TitlePage;
            foreach (Tuple<Tuple<string, Range>, FSharpList<FountainSpanElement>> k in titlePage.Item1)
            {
                KeyValuePairs.Add(new KeyValuePair(k));
            }
            Range = titlePage.Item2;
        }

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
    class Transition
    {
        public bool Forced { get; }
        public Spans Spans { get; }
        public Range Range { get; }
        public Transition(FountainBlockElement Transition)
        {
            FountainBlockElement.Transition transition = (FountainBlockElement.Transition)Transition;
            Forced = transition.Item1;
            Spans = new Spans(transition.Item2);
            Range = transition.Item3;
        }
    }

    class Spans
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

        public class Note
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public Note(FountainSpanElement Note)
            {
                FountainSpanElement.Note note = (FountainSpanElement.Note)Note;
                Spans = note.Item1;
                Range = note.Item2;
            }
        }
        public class Literal
        {
            public string Text { get; }
            public Range Range { get; }
            public Literal(FountainSpanElement Literal)
            {
                FountainSpanElement.Literal literal = (FountainSpanElement.Literal)Literal;
                Text = literal.Item1;
                Range = literal.Item2;
            }
        }
        public class Bold
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public Bold(FountainSpanElement Bold)
            {
                FountainSpanElement.Bold bold = (FountainSpanElement.Bold)Bold;
                Spans = bold.Item1;
                Range = bold.Item2;
            }
        }
        public class Italic
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public Italic(FountainSpanElement Italic)
            {
                FountainSpanElement.Italic italic = (FountainSpanElement.Italic)Italic;
                Spans = italic.Item1;
                Range = italic.Item2;
            }
        }
        public class Underline
        {
            public FSharpList<FountainSpanElement> Spans { get; }
            public Range Range { get; }
            public Underline(FountainSpanElement Underline)
            {
                FountainSpanElement.Underline underline = (FountainSpanElement.Underline)Underline;
                Spans = underline.Item1;
                Range = underline.Item2;
            }
        }
        public class HardLineBreak
        {
            public Range Range { get; }
            public HardLineBreak(FountainSpanElement HardLineBreak)
            {
                FountainSpanElement.HardLineBreak hardLineBreak = (FountainSpanElement.HardLineBreak)HardLineBreak;
                Range = hardLineBreak.Item;
            }
        }
    }
}

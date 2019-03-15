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

namespace WpfApp1
{
    class FountainGame
    {
        public FountainDocument Fountain { get; }
        public FSharpList<FountainBlockElement> Blocks => Fountain.Blocks;
        public string Html => HtmlFormatter.WriteHtml(Fountain);
        public List<FountainBlockElement.Action> Action => Blocks.Where((b) => b.IsAction).Cast<FountainBlockElement.Action>().ToList();
        public List<FountainBlockElement.Boneyard> Boneyard => Blocks.Where((b) => b.IsBoneyard).Cast<FountainBlockElement.Boneyard>().ToList();
        public List<FountainBlockElement.Centered> Centered => Blocks.Where((b) => b.IsCentered).Cast<FountainBlockElement.Centered>().ToList();
        public List<FountainBlockElement.Character> Character => Blocks.Where((b) => b.IsCharacter).Cast<FountainBlockElement.Character>().ToList();
        public List<FountainBlockElement.Dialogue> Dialogue => Blocks.Where((b) => b.IsDialogue).Cast<FountainBlockElement.Dialogue>().ToList();
        public List<FountainBlockElement.DualDialogue> DualDialogue => Blocks.Where((b) => b.IsDualDialogue).Cast<FountainBlockElement.DualDialogue>().ToList();
        public List<FountainBlockElement.Lyrics> Lyrics => Blocks.Where((b) => b.IsLyrics).Cast<FountainBlockElement.Lyrics>().ToList();
        public List<FountainBlockElement.PageBreak> PageBreak => Blocks.Where((b) => b.IsPageBreak).Cast<FountainBlockElement.PageBreak>().ToList();
        public List<FountainBlockElement.Parenthetical> Parenthetical => Blocks.Where((b) => b.IsParenthetical).Cast<FountainBlockElement.Parenthetical>().ToList();
        public List<FountainBlockElement.SceneHeading> SceneHeading => Blocks.Where((b) => b.IsSceneHeading).Cast<FountainBlockElement.SceneHeading>().ToList();
        public List<FountainBlockElement.Section> Section => Blocks.Where((b) => b.IsSection).Cast<FountainBlockElement.Section>().ToList();
        public List<FountainBlockElement.Synopses> Synopses => Blocks.Where((b) => b.IsSynopses).Cast<FountainBlockElement.Synopses>().ToList();
        public List<FountainBlockElement.TitlePage> TitlePage => Blocks.Where((b) => b.IsTitlePage).Cast<FountainBlockElement.TitlePage>().ToList();
        public List<FountainBlockElement.Transition> Transition => Blocks.Where((b) => b.IsTransition).Cast<FountainBlockElement.Transition>().ToList();
        public FountainGame(string Text)
        {
            Fountain = FountainDocument.Parse(Text);
        }
    }
}

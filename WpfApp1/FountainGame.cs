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
        public List<FountainBlockElement> Action => Blocks.Where((b) => b.IsAction).ToList();
        public List<FountainBlockElement> Boneyard => Blocks.Where((b) => b.IsBoneyard).ToList();
        public List<FountainBlockElement> Centered => Blocks.Where((b) => b.IsCentered).ToList();
        public List<FountainBlockElement> Character => Blocks.Where((b) => b.IsCharacter).ToList();
        public List<FountainBlockElement> Dialogue => Blocks.Where((b) => b.IsDialogue).ToList();
        public List<FountainBlockElement> DualDialogue => Blocks.Where((b) => b.IsDualDialogue).ToList();
        public List<FountainBlockElement> Lyrics => Blocks.Where((b) => b.IsLyrics).ToList();
        public List<FountainBlockElement> PageBreak => Blocks.Where((b) => b.IsPageBreak).ToList();
        public List<FountainBlockElement> Parenthetical => Blocks.Where((b) => b.IsParenthetical).ToList();
        public List<FountainBlockElement> SceneHeading => Blocks.Where((b) => b.IsSceneHeading).ToList();
        public List<FountainBlockElement> Section => Blocks.Where((b) => b.IsSection).ToList();
        public List<FountainBlockElement> Synopses => Blocks.Where((b) => b.IsSynopses).ToList();
        public List<FountainBlockElement> TitlePage => Blocks.Where((b) => b.IsTitlePage).ToList();
        public List<FountainBlockElement> Transition => Blocks.Where((b) => b.IsTransition).ToList();
        public FountainGame(string Text)
        {
            Fountain = FountainDocument.Parse(Text);
        }
    }
}

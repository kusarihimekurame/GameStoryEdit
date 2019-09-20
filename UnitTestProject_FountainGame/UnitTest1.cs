using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GameStoryEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject_FountainGame
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IHighlightingDefinition SyntaxHighlighting;
            FountainGame fountainGame = new FountainGame(GetText("ScreenPlay1.fountain"));

            using (XmlReader reader = XmlReader.Create("Fountain-Mode.xshd"))
            {
                SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            foreach(SceneHeading sceneHeading in fountainGame.Blocks.SceneHeadings)
            {
                Regex regex = SyntaxHighlighting.GetNamedRuleSet("SceneHeadings").Rules[0].Regex;
                string text = sceneHeading.Spans.Literals[0].Text;

                Assert.IsTrue(regex.IsMatch(text), regex.ToString() + "\r\n" + text);
            }

            foreach (Character character in fountainGame.Blocks.Characters)
            {
                Regex regex = SyntaxHighlighting.GetNamedRuleSet("").Spans[0].StartExpression;
                string text = character.Spans.Literals[0].Text.Trim();

                Assert.IsTrue(regex.IsMatch(text), regex.ToString() + "\r\n" + text);
            }

            foreach (Boneyard boneyard in fountainGame.Blocks.Boneyards)
            {
                Regex regex = SyntaxHighlighting.GetNamedRuleSet("Boneyard").Rules[0].Regex;
                string text = boneyard.Text;

                Assert.IsTrue(regex.IsMatch(text), regex.ToString() + "\r\n" + text);
            }

            foreach (Parenthetical parenthetical in fountainGame.Blocks.Parentheticals)
            {
                Regex regex = SyntaxHighlighting.GetNamedRuleSet("Parenthetical").Rules[0].Regex;
                string text = fountainGame.GetText(parenthetical.Range).Trim();

                Assert.IsTrue(regex.IsMatch(text), regex.ToString() + "\r\n" + text);
            }
        }

        public string GetText(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string _line;
                StringBuilder line = new StringBuilder();
                while ((_line = sr.ReadLine()) != null)
                {
                    line.AppendLine(_line);
                }
                return line.ToString();
            }
        }
    }
}

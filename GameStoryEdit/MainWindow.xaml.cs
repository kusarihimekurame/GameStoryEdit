using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Data;
using System.Globalization;
using ICSharpCode.AvalonEdit.Folding;
using System.Xml;
using System.Reflection;

namespace GameStoryEdit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        FountainGame FountainGame;
        private async Task<FountainGame> FountainGameAsync(string Text) => await Task.Run(() => new FountainGame(Text));

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer Timer = new DispatcherTimer();
            Timer.Tick += (sender, e) =>
            {
                Languages.languages.SetTextLocation(textEditor.Document.GetLocation(textEditor.SelectionStart + textEditor.SelectionLength));
                Languages.languages.SetLines(textEditor.LineCount);
                Languages.languages.SetCharacters(textEditor.Document.TextLength);
            };
            Timer.Start();

            using (XmlReader reader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("GameStoryEdit.Resources.HighLighting.Fountain-Mode.xshd")))
            {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private async void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextEditor text = (TextEditor)sender;
            FountainGame = await FountainGameAsync(text.Text);
            webBrowser.NavigateToString(FountainGame.Html);

            #region ListBox

            var Characters = FountainGame.Blocks.Characters.
                GroupBy(c => c.Spans.Literals[0].Text).
                Select(Group => new { Text = Group.Key, Count = Group.Count() }).ToList();

            var SceneHeadings = FountainGame.Blocks.SceneHeadings.
                Select(c => c.Spans.Literals[0].Text).ToList();

            CharactersListBox.ItemsSource = Characters;
            SceneHeadingListBox.ItemsSource = SceneHeadings;

            #endregion

            //HTMLToPdf(FountainGame.Html, @"F:\GameStory.pdf");
            //webBrowser.NavigateToString(text.Text);  //测试html语句用
        }

        public void HTMLToPdf(string HTML, string FilePath)
        {
            #region HTML to Pdf

            Document document = new Document();
            TextReader textReader=new StringReader(HTML);
            PdfWriter pdfWriter = PdfWriter.GetInstance(document, new FileStream(FilePath, FileMode.Create));
            document.Open();
            XMLWorkerHelper.GetInstance().ParseXHtml(pdfWriter, document, textReader);
            document.Close();
            pdfWriter.Close();
            textReader.Close();

            #endregion
        }

        private void SceneHeadingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                foreach (var c in FountainGame.Blocks.SceneHeadings)
                    c.Spans.Literals.Where(s => s.Text == e.AddedItems[0].ToString()).ToList().ForEach(s =>
                    {
                        textEditor.Select(s.Range.Location, s.Range.Length);
                        textEditor.ScrollToLine(textEditor.Document.GetLineByOffset(s.Range.Location).LineNumber);
                    });
            }
        }
    }
}

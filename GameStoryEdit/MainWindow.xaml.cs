using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
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

namespace GameStoryEdit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        FountainGame FountainGame;
        private async Task<FountainGame> FountainGameAsync(string Text) => await Task.Run(() => new FountainGame(Text));
        private TextLocation TextLocation;
        private TEXTLocation tl = new TEXTLocation();
        private int SelectionStart
        {
            get
            {
                tl.TextLocation = textEditor.Document.GetLocation(textEditor.SelectionStart);
                return textEditor.SelectionStart;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            textLocation_Ln.DataContext = tl;
            textLocation_Col.DataContext = tl;
        }

        private async void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextEditor text = (TextEditor)sender;
            FountainGame = await FountainGameAsync(text.Text);
            webBrowser.NavigateToString(FountainGame.Html);

            var Characters = FountainGame.Blocks.Characters.
                GroupBy(c => FountainGame.GetText(c.Range).Trim()).
                Select(Group => new { Text = Group.Key, Count = Group.Count() }).ToList();

            var SceneHeadings = FountainGame.Blocks.SceneHeadings.
                Select(c => FountainGame.GetText(c.Range).Trim()).ToList();

            CharactersListBox.ItemsSource = Characters;
            SceneHeadingListBox.ItemsSource = SceneHeadings;

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

    class TEXTLocation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private TextLocation textLocation;
        public TextLocation TextLocation
        {
            get => textLocation;
            set
            {
                textLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextLocation"));
            }
        }
        public int Line => TextLocation.Line;
        public int Column => textLocation.Column;
    }
}

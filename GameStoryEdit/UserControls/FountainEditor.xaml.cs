using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace GameStoryEdit.UserControls
{
    /// <summary>
    /// TextEditor.xaml 的交互逻辑
    /// </summary>
    public partial class FountainEditor : UserControl
    {
        public FountainGame FountainGame
        {
            get { return fountainGame; }
            set
            {
                fountainGame = value;

                FountainGame_Changed?.Invoke(fountainGame, null);

                webBrowser.NavigateToString(fountainGame.Html);

                #region Folding

                List<NewFolding> newFoldings = new List<NewFolding>();
                fountainGame.SceneBlocks.ForEach(sb =>
                {
                    try
                    {
                        newFoldings.Add(new NewFolding(sb.Range.Location, sb.Range.EndLocation - 1) { Name = sb.SceneHeadings[0].Spans.Literals[0].Text });
                        sb.DialogueBlocks.ForEach(db => newFoldings.Add(new NewFolding(db.Range.Location + 2, db.Range.EndLocation - 1) { Name = db.Characters[0].Spans.Literals[0].Text }));
                    }
                    catch { }
                });
                foldingManager.UpdateFoldings(newFoldings.Cast<NewFolding>(), -1);
                foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);

                #endregion

                //HTMLToPdf(FountainGame.Html, @"F:\GameStory.pdf");
                //webBrowser.NavigateToString(text.Text);  //测试html语句用
            }
        }

        public delegate void _FountainGame_Changed(object sender, EventArgs e);
        public event _FountainGame_Changed FountainGame_Changed;
        public WebBrowser webBrowser = new WebBrowser();

        private FoldingManager foldingManager;
        private XmlFoldingStrategy foldingStrategy;
        private FountainGame fountainGame;

        private DispatcherTimer Timer = new DispatcherTimer();

        public FountainEditor()
        {
            InitializeComponent();

            Timer.Tick += (sender, e) =>
            {
                Languages.languages.SetTextLocation(textEditor.Document.GetLocation(textEditor.SelectionStart + textEditor.SelectionLength));
                Languages.languages.SetLines(textEditor.LineCount);
                Languages.languages.SetCharacters(textEditor.Document.TextLength);
            };
            
            using (XmlReader reader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("GameStoryEdit.HighLighting.Fountain-Mode.xshd")))
            {
                textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            foldingManager = FoldingManager.Install(textEditor.TextArea);
            foldingStrategy = new XmlFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        }

        private async void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor text)
            {
                FountainGame = await FountainGame.GetValueAsync(text.Text);
            }
        }

        public void HTMLToPdf(string HTML, string FilePath)
        {
            #region HTML to Pdf

            Document document = new Document();
            TextReader textReader = new StringReader(HTML);
            PdfWriter pdfWriter = PdfWriter.GetInstance(document, new FileStream(FilePath, FileMode.Create));
            document.Open();
            XMLWorkerHelper.GetInstance().ParseXHtml(pdfWriter, document, textReader);
            document.Close();
            pdfWriter.Close();
            textReader.Close();

            #endregion
        }

        private void TextEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Timer.Start();
            }
            else
            {
                Timer.Stop();
                Languages.languages.SetTextLocation(new TextLocation(0, 0));
                Languages.languages.SetLines(0);
                Languages.languages.SetCharacters(0);
            }
        }
    }
}

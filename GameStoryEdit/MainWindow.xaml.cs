using GameStoryEdit.UserControls;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace GameStoryEdit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        FountainEditor FountainEditor => LayoutDocumentPane.SelectedContent.Content as FountainEditor;
        FountainGame FountainGame => FountainEditor.FountainGame;
        XmlLayoutSerializer serializer => new XmlLayoutSerializer(dockingManager);
        LayoutPanel LayoutPanel => (LayoutPanel)dockingManager.Layout.Children.FirstOrDefault();
        LayoutDocumentPane LayoutDocumentPane => (LayoutDocumentPane)((LayoutPanel)dockingManager.Layout.Children.FirstOrDefault()).Children.FirstOrDefault(c => c is LayoutDocumentPane);

        public MainWindow()
        {
            InitializeComponent();

            LayoutDocument ld = new LayoutDocument() { Content = new FountainEditor(), ContentId = "FountainEditor" };
            ((FountainEditor)ld.Content).FountainGame_Changed += FountainGame_Changed;
            LayoutDocumentPane.Children.Add(ld);

            if (File.Exists(@".gse\Layout.xml"))
                using (XmlReader Reader = XmlReader.Create(@".gse\Layout.xml"))
                {
                    serializer.Deserialize(Reader);
                }

            LayoutDocumentPane.Children.Remove(ld);
        }

        private void Add_Html(object sender, RoutedEventArgs e)
        {
            LayoutPanel.Children.Insert(2, new LayoutDocumentPane(new LayoutDocument() { Content = FountainEditor.webBrowser, Title = "HTML" }));
        }

        private void SceneHeadingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                foreach (var c in FountainGame.Blocks.SceneHeadings)
                    c.Spans.Literals.Where(s => s.Text == e.AddedItems[0].ToString()).ToList().ForEach(s =>
                    {
                        FountainEditor.textEditor.Select(s.Range.Location, s.Range.Length);
                        FountainEditor.textEditor.ScrollToLine(FountainEditor.textEditor.Document.GetLineByOffset(s.Range.Location).LineNumber);
                    });
            }
        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;

            LayoutDocument ld = LayoutDocumentPane.Children.Cast<LayoutDocument>().SingleOrDefault(d => d.Title == tvi.Header.ToString());
            if (ld != null)
            {
                ld.IsSelected = true;
                ld.IsActive = true;
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;

            if (LayoutDocumentPane.Children.Cast<LayoutDocument>().Count(d => d.Title == tvi.Header.ToString()) == 0)
            {
                FountainEditor fe = new FountainEditor();
                LayoutDocumentPane.Children.Add(new LayoutDocument() { Content = fe, ContentId = "FountainEditor", Title = tvi.Header.ToString() });
                fe.FountainGame_Changed += FountainGame_Changed;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Directory.Exists(".gse")) Directory.CreateDirectory(".gse").Attributes = FileAttributes.Hidden;
            using (var stream = new StreamWriter(@".gse\Layout.xml"))
            {
                serializer.Serialize(stream);
            }
        }

        private void FountainGame_Changed(object sender, EventArgs e)
        {
            #region ListBox

            FountainGame fountainGame = sender as FountainGame;

            var Characters = fountainGame.Blocks.Characters.
                GroupBy(c => c.Spans.Literals[0].Text).
                Select(Group => new { Text = Group.Key, Count = Group.Count() }).ToList();

            var SceneHeadings = fountainGame.Blocks.SceneHeadings.
                Select(c => c.Spans.Literals[0].Text).ToList();

            CharactersListBox.ItemsSource = Characters;
            SceneHeadingListBox.ItemsSource = SceneHeadings;

            #endregion
        }
    }
}

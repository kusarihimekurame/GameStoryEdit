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
using GameStoryEdit.TreeData;
using System.ComponentModel;
using System.Collections.Generic;

namespace GameStoryEdit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public FountainEditor FountainEditor
        {
            get
            {
                LayoutDocument ld = Layouts_FountainEditor.FirstOrDefault(lf => lf.IsSelected);
                return ld == null ? null : (FountainEditor)ld.Content;
            }
        }

        public FountainGame FountainGame => FountainEditor.FountainGame;
        public XmlLayoutSerializer serializer => new XmlLayoutSerializer(dockingManager);
        public LayoutPanel LayoutPanel => (LayoutPanel)dockingManager.Layout.Children.FirstOrDefault();
        public LayoutDocumentPane LayoutDocumentPane => (LayoutDocumentPane)((LayoutPanel)dockingManager.Layout.Children.FirstOrDefault()).Children.FirstOrDefault(c => c is LayoutDocumentPane);
        public List<LayoutDocument> Layouts_FountainEditor => dockingManager.Layout.Descendents().OfType<LayoutDocument>().Where(ld => !string.IsNullOrEmpty(ld.ContentId) && ld.ContentId.Equals("FountainEditor")).ToList();

        public MainWindow()
        {
            InitializeComponent();

            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Layout\";

            if (File.Exists(path + TreeItem.Solution.Name + ".xml"))
            {
                using (XmlReader Reader = XmlReader.Create(path + TreeItem.Solution.Name + ".xml"))
                {
                    serializer.Deserialize(Reader);
                }
            }

            Layouts_FountainEditor.ForEach(lf =>
            {
                if (TreeView.DataContext is SolutionViewModel solutionView)
                {
                    lf.Content = solutionView.FindMatches(lf.Title, solutionView.Projects).Select(vm => vm.TreeItem).Cast<ScreenPlay>().First().FountainEditor;
                }
            });

            if (FountainEditor != null) FountainEditor.FountainGame_Changed += FountainGame_Changed;
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Layout\";

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (StreamWriter stream = new StreamWriter(path + TreeItem.Solution.Name + ".xml"))
            {
                serializer.Serialize(stream);
            }
        }

        public void FountainGame_Changed(object sender, EventArgs e)
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

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
        }
    }
}

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
        public LayoutAnchorable Manager => dockingManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(la => !string.IsNullOrEmpty(la.ContentId) && la.ContentId.Equals("Manager"));

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

            close.CommandParameter = LayoutDocumentPane;
            LayoutDocumentPane.ChildrenCollectionChanged += (sender, e) => ((Commands.Command._Close)close.Command).RaiseCanExecuteChanged();

            Layouts_FountainEditor.ForEach(lf =>
            {
                if (TreeView.DataContext is SolutionViewModel solutionView)
                {
                    lf.Content = solutionView.FindMatches(lf.Title, solutionView.Projects).Select(vm => vm.TreeItem).Cast<ScreenPlay>().First().FountainEditor;
                    Manager.Content = ((FountainEditor)lf.Content).Manager;
                }

                lf.IsSelectedChanged += (sender, e) =>
                {
                    if (lf.IsSelected && Manager != null)
                    {
                        Manager.Content = ((FountainEditor)lf.Content).Manager;
                    }
                };
            });
        }

        private void Add_Html(object sender, RoutedEventArgs e)
        {
            LayoutPanel.Children.Insert(2, new LayoutDocumentPane(new LayoutDocument() { Content = FountainEditor.webBrowser, Title = "HTML" }) { DockWidth = LayoutDocumentPane.DockWidth });
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

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow = this;
        }
    }
}

using GameStoryEdit.TreeData;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Xceed.Wpf.AvalonDock.Layout;

namespace GameStoryEdit.UserControls
{
    /// <summary>
    /// TreeView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeView : UserControl
    {
        public TreeView()
        {
            InitializeComponent();

            // Let the UI bind to the view-model.
            DataContext = TreeItem.GetSolutionTree();
        }

        public void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem tvi && tvi.Header is TreeViewItemViewModel viewModel)
            {
                LayoutDocument ld = ((MainWindow)Application.Current.MainWindow).Layouts_FountainEditor.SingleOrDefault(d => d.Title == viewModel.Name);
                if (ld != null)
                {
                    ld.IsSelected = true;
                    ld.IsActive = true;
                }
            }
        }

        public void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem tvi && tvi.Header is TreeViewItemViewModel viewModel)
            {
                if (((MainWindow)Application.Current.MainWindow).Layouts_FountainEditor.Count(d => d.Title == viewModel.Name) == 0)
                {
                    switch(viewModel.TreeItem)
                    {
                        case ScreenPlay screenPlay:
                            ((MainWindow)Application.Current.MainWindow).LayoutDocumentPane.Children.Add(screenPlay.LayoutDocument);
                            screenPlay.FountainEditor.FountainGame_Changed += ((MainWindow)Application.Current.MainWindow).FountainGame_Changed;
                            break;
                    }
                }
            }
        }
    }
}

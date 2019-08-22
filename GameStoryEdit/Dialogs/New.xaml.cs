using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using GameStoryEdit.TreeData;

namespace GameStoryEdit.Dialogs
{
    /// <summary>
    /// New.xaml 的交互逻辑
    /// </summary>
    public partial class New : Window
    {
        private string path
        {
            get => Path.Text;
            set
            {
                Path.Text = value;

                int i = 1;
                string name = "GameStory1";
                while (Directory.Exists(value + @"\" + name))
                {
                    i++;
                    name = name.Substring(0, name.Length - (i - 1).ToString().Length) + i;
                }
                if (string.IsNullOrWhiteSpace(ProjectName.Text))
                {
                    TextBox_PreviewTextInput(ProjectName, null);
                    ProjectName.Text = name;
                }
            }
        }

        public New()
        {
            InitializeComponent();
            Path.TextChanged += (sender, e) => { if (sender is TextBox tb) path = Path.Text; };
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        bool Text_Equals;
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Text_Equals = tb.Text.Equals(SolutionName.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text_Equals) SolutionName.Text = (sender as TextBox).Text;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Button_GotFocus(object sender, RoutedEventArgs e)
        {
            ((Control)sender).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
        }

        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            ((Control)sender).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333337"));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Solution solution = new Solution() { Name = SolutionName.Text, Path = path + @"\" + SolutionName.Text };

            Project project = new Project() { Name = ProjectName.Text, Path = solution.Path + @"\" + ProjectName.Text };
            Assets assets = new Assets() { Name = "Assets", Path = project.Path + @"\Assets" };
            assets.Children.Add(new BaseTreeItem() { Name = "Audio", Path = assets.Path + @"\Audio" });
            assets.Children.Add(new BaseTreeItem() { Name = "Documents", Path = assets.Path + @"\Documents" });
            assets.Children.Add(new BaseTreeItem() { Name = "Images", Path = assets.Path + @"\Images" });
            assets.Children.Add(new BaseTreeItem() { Name = "Videos", Path = assets.Path + @"\Videos" });
            Document document = new Document() { Name = "Document", Path = assets.Children["Documents"].Path };
            ScreenPlay screenPlay = new ScreenPlay() { Name = "ScreenPlay1", Path = document.Path };
            document.Children.Add(screenPlay);
            Flow flow = new Flow() { Name = "Flow", Path = project.Path + @"\Flow" };
            Locations locations = new Locations() { Name = "Locations" };
            Tables tables = new Tables() { Name = "Tables", Path = project.Path + @"\Tables" };
            project.Children.Add(document);
            project.Children.Add(flow);
            project.Children.Add(locations);
            project.Children.Add(tables);
            project.Children.Add(assets);
            solution.Projects.Add(project);

            screenPlay.FountainEditor = new UserControls.FountainEditor();
            solution.Serialize();
            solution = Solution.Deserialize(solution.Path + @"\" + solution.Name + ".gse");
            
            if (File.Exists(@"Layout\" + solution.Name + ".xml")) File.Delete(@"Layout\" + solution.Name + ".xml");

            TreeItem.Solution = solution;
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
            Close();
        }
    }
}

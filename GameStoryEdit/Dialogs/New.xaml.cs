﻿using System;
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
using GameStoryEdit.Date;

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
            Text_Equals = tb.Text.Equals(GameName.Text);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text_Equals) GameName.Text = (sender as TextBox).Text;
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
            string ProjectDirectory = path + @"\" + ProjectName.Text;
            string ProjectFile = ProjectDirectory + @"\" + ProjectName.Text + ".gse";
            List<string> GameDirectory = new List<string>() { ProjectDirectory + @"\" + GameName.Text };
            List<string> GameFile = new List<string>() { GameDirectory + @"\" + GameName.Text + ".GameStory" };
            ProjectPath = new ProjectPath() { ProjectDirectory = ProjectDirectory, ProjectFile = ProjectFile, GameDirectory = GameDirectory, GameFile = GameFile };

            Directory.CreateDirectory(ProjectPath.ProjectDirectory);
            File.Create(ProjectPath.ProjectFile).Close();
            Directory.CreateDirectory(ProjectPath.GameDirectory[0]);
            File.Create(ProjectPath.GameFile[0]).Close();

            Application.Current.MainWindow = new MainWindow(ProjectPath);
            Application.Current.MainWindow.Show();
            Close();
        }
        public ProjectPath ProjectPath { get; private set; }
    }
}
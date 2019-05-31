using GameStoryEdit.Date;
using GameStoryEdit.Dialogs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameStoryEdit.Commands
{
    public partial class Command
    {
        public static ICommand Exit { get; } = new _Exit();
        public static ICommand NewDialog { get; } = new _NewDialog();
        public static ICommand Open { get; } = new _Open();
        public static ICommand OpenDialog { get; } = new _OpenDialog();
        public static ICommand Close { get; } = new _Close();

        private class _Exit : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                Application.Current.MainWindow.Close();
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class _NewDialog : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                if (parameter == null) new New().ShowDialog();
                else if (parameter.ToString().Equals("project", StringComparison.OrdinalIgnoreCase))
                {
                    New dialog = new New();
                    dialog.tabControl.SelectedIndex = 1;
                    dialog.ShowDialog();
                }
                else if (parameter is Control cl)
                {
                    TabControl tabControl = (Window.GetWindow(cl) as New).tabControl;
                    if (cl is Button button)
                        if (button.Content.ToString() == "上一步") tabControl.SelectedIndex -= 1;
                        else if (button.Content.ToString() == "下一步") tabControl.SelectedIndex += 1;
                        else tabControl.SelectedIndex = 1;
                }
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class _Open : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "打开项目";
                ofd.Filter = "gse项目文件|*.gse";
                if (ofd.ShowDialog().GetValueOrDefault())
                {
                    string ProjectDirectory = Path.GetDirectoryName(ofd.FileName);
                    string ProjectFile = ofd.FileName;
                    List<string> GameDirectory = new List<string>();
                    List<string> GameFile = new List<string>();
                    DirectoryInfo dir = new DirectoryInfo(ProjectDirectory);
                    FileSystemInfo[] files = dir.GetFileSystemInfos("*.GameStory", SearchOption.AllDirectories);
                    files.Cast<FileInfo>().ToList().ForEach(f => { GameDirectory.Add(f.DirectoryName); GameFile.Add(f.FullName); });
                    ProjectPath ProjectPath = new ProjectPath() { ProjectDirectory = ProjectDirectory, ProjectFile = ProjectFile, GameDirectory = GameDirectory, GameFile = GameFile };

                    Application.Current.MainWindow = new MainWindow(ProjectPath);
                    Application.Current.MainWindow.Show();
                }
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class _OpenDialog : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                TextBox tb = parameter as TextBox;
                CommonOpenFileDialog FileDialog = new CommonOpenFileDialog("项目位置") { IsFolderPicker = true, DefaultDirectory = tb.Text };
                if (FileDialog.ShowDialog(Window.GetWindow(parameter as TextBox)) == CommonFileDialogResult.Ok) tb.Text = FileDialog.FileName;
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private class _Close : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                Window.GetWindow((DependencyObject)parameter).Close();
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

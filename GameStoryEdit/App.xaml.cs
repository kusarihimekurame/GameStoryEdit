using GameStoryEdit.Commands;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using GameStoryEdit.Dialogs;
using System.Windows.Threading;

namespace GameStoryEdit
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] arguments = e.Args;

            if (arguments.Length > 0 && arguments[0].EndsWith(".gse"))
            {
                string path = arguments[0];
                Command.Open.Execute(path);
            }

            if (MainWindow == null) new New().ShowDialog();

            Languages.languages.SetLanguage(CultureInfo.CurrentCulture);
        }
        
        private void Application_Activated(object sender, EventArgs e)
        {
            MainWindow_InputBindings.Add();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }
}

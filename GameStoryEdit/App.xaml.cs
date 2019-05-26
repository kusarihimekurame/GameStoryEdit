using GameStoryEdit.Commands;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace GameStoryEdit
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Languages.languages.SetLanguage(CultureInfo.CurrentCulture);
        }
        
        private void Application_Activated(object sender, EventArgs e)
        {
            MainWindow_InputBindings.Add();
        }
    }
}

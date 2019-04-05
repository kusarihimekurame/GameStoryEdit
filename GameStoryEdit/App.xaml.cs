using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameStoryEdit
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            switch(CultureInfo.CurrentCulture.Name.ToLower())
            {
                case "en-us":
                    Languages.languages.SetLanguage(LanguageOptions.en_us);
                    break;
                case "zh-ch":
                    Languages.languages.SetLanguage(LanguageOptions.ch_zh);
                    break;
                case "ja-jp":
                    Languages.languages.SetLanguage(LanguageOptions.ja_jp);
                    break;
                default:
                    goto case "en-us";
            }
        }
    }
}

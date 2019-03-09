using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        FountainGame FountainGame;
        private async Task<FountainGame> FountainGameAsync(string Text) => await Task.Run(() => new FountainGame(Text));

        public MainWindow()
        {
            InitializeComponent(); 
        }

        private async void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextEditor text = (TextEditor)sender;
            FountainGame = await FountainGameAsync(text.Text);
            webBrowser.NavigateToString(FountainGame.Html);
            

            #region Auto HighLight Text



            #endregion
        }
    }
}

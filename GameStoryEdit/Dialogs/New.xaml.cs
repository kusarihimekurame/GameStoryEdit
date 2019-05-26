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
using System.Windows.Shapes;

namespace GameStoryEdit.Dialogs
{
    /// <summary>
    /// New.xaml 的交互逻辑
    /// </summary>
    public partial class New : Window
    {
        public New()
        {
            InitializeComponent();
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
    }
}

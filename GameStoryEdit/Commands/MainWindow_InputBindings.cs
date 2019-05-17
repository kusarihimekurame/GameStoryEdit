using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GameStoryEdit.Commands
{
    public class MainWindow_InputBindings : Command
    {
        public static void Add()
        {
            Application.Current.MainWindow.InputBindings.Add(new InputBinding(NewDialog, new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift)));
        }
    }
}

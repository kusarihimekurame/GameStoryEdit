using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameStoryEdit
{
    [Serializable]
    public partial class Languages : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public static Languages languages { get; } = new Languages();
        public ICommand Language_Changed { get; } = new _Language_Changed();
        public string LocationString { get; private set; } = string.Format(Application.Current.FindResource("LocationString").ToString(), 0, 0);
        public string Lines { get; private set; } = string.Format(Application.Current.FindResource("Lines").ToString(), 0);
        public string Characters { get; private set; } = string.Format(Application.Current.FindResource("Characters").ToString(), 0);
        public CultureInfo Language { get; private set; }

        private int Line;
        private int Column;
        private int linesNum;
        private int charactersNum;

        public void SetLines(int value)
        { linesNum = value; SetLanguage(Language); }
        public void SetCharacters(int value)
        { charactersNum = value; SetLanguage(Language); }
        public void SetTextLocation(TextLocation value)
        {
            Line = value.Line;
            Column = value.Column;
            SetLanguage(Language);
        }

        public void SetLanguage(CultureInfo value)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = string.Format(@"Resources\Language\{0}.xaml", value);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            Language = value;

            LocationString = string.Format(Application.Current.FindResource("LocationString").ToString(), Line, Column);
            Lines = string.Format(Application.Current.FindResource("Lines").ToString(), linesNum);
            Characters = string.Format(Application.Current.FindResource("Characters").ToString(), charactersNum);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocationString"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lines"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Characters"));
        }

        private class _Language_Changed : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                languages.SetLanguage(new CultureInfo(parameter.ToString()));
            }
        }
    }
}

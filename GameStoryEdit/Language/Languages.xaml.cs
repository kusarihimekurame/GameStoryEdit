using GameStoryEdit.TreeData;
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
    public partial class Languages : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static Languages languages { get; } = new Languages();
        public ICommand Language_Changed { get; } = new _Language_Changed();
        public string LocationString { get; private set; } = string.Format(Application.Current.FindResource("LocationString").ToString(), 0, 0);
        public string Lines { get; private set; } = string.Format(Application.Current.FindResource("Lines").ToString(), 0);
        public string Characters { get; private set; } = string.Format(Application.Current.FindResource("Characters").ToString(), 0);
        public string Solution { get; private set; } = string.Format(Application.Current.FindResource("Solution").ToString(), TreeItem.Solution.Name, TreeItem.Solution.Projects.Count);
        public CultureInfo Language { get; private set; }
        public bool IsEnglish { get; private set; }
        public bool IsChinese_Simplified { get; private set; }
        public bool IsJapanese { get; private set; }

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
            string requestedCulture = string.Format(@"Language\{0}.xaml", value);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            Language = value;

            switch (value.Name)
            {
                case "zh-CN":
                    IsEnglish = false;
                    IsChinese_Simplified = true;
                    IsJapanese = false;
                    break;
                case "ja-JP":
                    IsEnglish = false;
                    IsChinese_Simplified = false;
                    IsJapanese = true;
                    break;
                default:
                    IsEnglish = true;
                    IsChinese_Simplified = false;
                    IsJapanese = false;
                    break;
            }

            LocationString = string.Format(Application.Current.FindResource("LocationString").ToString(), Line, Column);
            Lines = string.Format(Application.Current.FindResource("Lines").ToString(), linesNum);
            Characters = string.Format(Application.Current.FindResource("Characters").ToString(), charactersNum);
            Solution = string.Format(Application.Current.FindResource("Solution").ToString(), TreeItem.Solution.Name, TreeItem.Solution.Projects.Count);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocationString"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lines"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Characters"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnglish"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChinese_Simplified"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsJapanese"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Solution"));
        }

        private class _Language_Changed : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                languages.SetLanguage(new CultureInfo(parameter.ToString()));
            }
            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public string LocationString { get; private set; }
        public string Lines { get; private set; }
        public string Characters { get; private set; }
        public string SceneHeading { get; private set; }
        public string Character { get; private set; }
        public LanguageOptions Language { get; private set; }

        private int Line;
        private int Column;
        private int linesNum;
        private int charactersNum;

        public void SetLines(int value)
        { linesNum = value; SetLanguage(Language); }
        public void SetCharacters(int value)
        { charactersNum = value; SetLanguage(Language); }
        public void SetTextLocation(TextLocation value)
        { Line = value.Line; Column = value.Column; SetLanguage(Language); }

        public void SetLanguage(LanguageOptions value)
        {
            switch (value)
            {
                case LanguageOptions.en_us:
                    LocationString = string.Format("Ln:{0},Col:{1}", Line, Column);
                    Lines = string.Format("{0} Lines", linesNum);
                    Characters = string.Format("{0} Characters", charactersNum);
                    SceneHeading = "SceneHeading";
                    Character = "Character";
                    break;
                case LanguageOptions.ch_zh:
                    LocationString = string.Format("第{0}行,第{1}列", Line, Column);
                    Lines = string.Format("共{0}行", linesNum);
                    Characters = string.Format("{0}个字符", charactersNum);
                    SceneHeading = "场景标题";
                    Character = "角色";
                    break;
                case LanguageOptions.ja_jp:
                    LocationString = string.Format("{0}行、{1}列", Line, Column);
                    Lines = string.Format("行数:{0}", linesNum);
                    Characters = string.Format("文字数:{0}", charactersNum);
                    SceneHeading = "見出しシーン";
                    Character = "キャラクター";
                    break;
            }
            Language = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Language"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocationString"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lines"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Characters"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SceneHeading"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Character"));
        }

        private class _Language_Changed : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter)
            {
                languages.SetLanguage((LanguageOptions)int.Parse(parameter.ToString()));
            }
        }
    }
    public enum LanguageOptions { en_us, ch_zh,ja_jp };
}

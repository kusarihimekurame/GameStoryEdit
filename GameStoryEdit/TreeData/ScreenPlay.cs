using GameStoryEdit.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xceed.Wpf.AvalonDock.Layout;

namespace GameStoryEdit.TreeData
{
    public class ScreenPlay : BaseTreeItem
    {
        public string Extension;
        public string FileName => Name + Extension;
        public string FullName => Path + @"\" + Name + Extension;

        private FountainEditor _fountainEditor;
        public FountainEditor FountainEditor
        {
            get => _fountainEditor;
            set
            {
                _fountainEditor = value;
                _fountainEditor.Name = Name;
                _fountainEditor.textEditor.Text = GetText();
                LayoutDocument = new LayoutDocument() { Content = _fountainEditor, ContentId = "FountainEditor", Title = Name };
            }
        }
        public LayoutDocument LayoutDocument { get; private set; }
        public string GetText()
        {
            Extension = new DirectoryInfo(Path).GetFiles().First(file => file.Name.Contains(Name)).Extension;
            using (StreamReader sr = new StreamReader(FullName, Encoding.Default))
            {
                string _line;
                StringBuilder line = new StringBuilder();
                while ((_line = sr.ReadLine()) != null)
                {
                    line.AppendLine(_line);
                }
                return line.ToString();
            }
        }
    }
}

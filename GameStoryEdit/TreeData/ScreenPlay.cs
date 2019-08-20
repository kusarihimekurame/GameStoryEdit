using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    public class ScreenPlay : BaseTreeItem
    {
        public string GetText()
        {
            StreamReader sr = new StreamReader(Path + @"\" + Name + ".fountain", Encoding.Default);
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    public class Document : ITreeItem
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ObservableCollection<ITreeItem> Children { get; } = new ObservableCollection<ITreeItem>();
    }
}

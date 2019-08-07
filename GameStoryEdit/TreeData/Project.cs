using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    [Serializable]
    public class Project : ITreeItem
    {
        private ITreeItem parent;

        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ITreeItem Parent
        {
            get => parent;
            set
            {
                if (parent != null && value != null) parent.Children.Remove(this);
                parent = value;
                if (parent != null) parent.Children.Add(this);
            }
        }
        [XmlIgnore]
        public ObservableCollection<ITreeItem> Children { get; } = new ObservableCollection<ITreeItem>();
    }
}

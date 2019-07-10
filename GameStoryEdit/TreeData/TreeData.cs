using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    public class SolutionPath
    {
        public string ProjectDirectory { get; set; }
        public string ProjectFile { get; set; }
        public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFile);
        public List<string> GameDirectory { get; set; }
        public List<string> GameFile { get; set; }
        public List<string> GameName => GameFile.Select(s => Path.GetFileNameWithoutExtension(s)).ToList();
    }

    [Serializable]
    public class TreeRoot : ITreeItem, IXmlSerializable
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ITreeItem Parent { get; } = null;
        public IEnumerable<ITreeItem> Children { get; } = new List<ITreeItem>();
        public XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        { }
        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteElementString("k0", k0.ToString());
            //writer.WriteElementString("k1", k1.ToString());
            //writer.WriteElementString("k2", k2.ToString());
        }
    }

    public interface ITreeItem
    {
        [XmlAttribute("Name")]
        string Name { get; set; }
        [XmlAttribute("Path")]
        string Path { get; set; }
        [XmlIgnore]
        ITreeItem Parent { get; }
        IEnumerable<ITreeItem> Children { get; }
    }

    [Serializable]
    [XmlInclude(typeof(Project))]
    public abstract class Solution : ITreeItem
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ITreeItem Parent { get; } = null;
        public abstract IEnumerable<ITreeItem> Children { get; }
    }

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
                parent = value;
                //parent.Children.Add(this);
            }
        }
        public IEnumerable<ITreeItem> Children { get; } = new List<ITreeItem>();
    }

    public class SolutionViewModel : TreeViewItemViewModel
    {
        public SolutionViewModel(Solution solution)
            : base(solution)
        {
        }
    }
    public class ProjectViewModel : TreeViewItemViewModel
    {
        public ProjectViewModel(Project project)
            : base(project)
        {
        }
    }
}

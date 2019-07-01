using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
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
    public class Solution : ITreeItem
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ITreeItem Parent { get; } = null;
        public IEnumerable<ITreeItem> Children { get; } = new List<ITreeItem>();
    }

    [Serializable]
    public class Project : ITreeItem
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlIgnore]
        public ITreeItem Parent { get; set; }
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

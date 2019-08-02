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

    public interface ITreeItem
    {
        [XmlAttribute("Name")]
        string Name { get; set; }
        [XmlAttribute("Path")]
        string Path { get; set; }
        [XmlIgnore]
        ITreeItem Parent { get; set; }
        ObservableCollection<ITreeItem> Children { get; }
    }

    [Serializable]
    public class Solution : IXmlSerializable
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public string Name { get; set; }
        public string Path { get; set; }

        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Solution");
            while (reader.Read())
            {
                reader.MoveToContent();

                if (reader.LocalName.Equals("Solution") && reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.IsEmptyElement || reader.NodeType == XmlNodeType.EndElement)
                {
                    continue;
                }

                switch (reader.LocalName)
                {
                    case "Name":
                        reader.Read();
                        Name = reader.Value;
                        break;
                    case "Path":
                        reader.Read();
                        Path = reader.Value;
                        break;
                }
            }

        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Solution");
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Path", Path);
            writer.WriteEndElement();

            writer.WriteStartElement("Projects");
            Projects.ForEach(p =>
            {
                writer.WriteStartElement(p.Name);
                writer.WriteElementString("Path", p.Path);
                writer.WriteEndElement();
            });
            writer.WriteEndElement();

            //writer.WriteElementString("Name", Name);
            //writer.WriteElementString("Path", Path);
            //writer.WriteStartElement("Project");
            //foreach (var child in Children)
            //{
            //    Type type = child.GetType();
            //    XmlSerializer serializer = new XmlSerializer(type);
            //    serializer.Serialize(writer, child);
            //}
            //writer.WriteEndElement();
        }
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
                if (parent != null) parent.Children.Add(this);
            }
        }
        [XmlIgnore]
        public ObservableCollection<ITreeItem> Children { get; } = new ObservableCollection<ITreeItem>();
    }

    //public class SolutionViewModel : TreeViewItemViewModel
    //{
    //    public SolutionViewModel(Solution solution)
    //        : base(solution)
    //    {
    //    }
    //}
    public class ProjectViewModel : TreeViewItemViewModel
    {
        public ProjectViewModel(Project project)
            : base(project)
        {
        }
    }
}

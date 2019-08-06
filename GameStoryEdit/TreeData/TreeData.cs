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
        public ProjectCollection Projects { get; set; } = new ProjectCollection();
        public string Name { get; set; }
        public string Path { get; set; }

        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
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
                    case "Projects":
                        while (reader.Read())
                        {
                            if (reader.LocalName.Equals("Projects") && reader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }

                            if (reader.NodeType == XmlNodeType.Element && !reader.LocalName.Equals("Path"))
                            {
                                string name = reader.LocalName;
                                reader.Read();
                                reader.Read();

                                XmlSerializer serializer = new XmlSerializer(typeof(Project));
                                using (XmlReader xmlReader = XmlReader.Create(reader.Value + @"\" + name + ".GameStory"))
                                {
                                    Projects.Add((Project)serializer.Deserialize(xmlReader));
                                }
                            }
                        }
                        break;
                }
            }

        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Path", Path);

            writer.WriteStartElement("Projects");
            Projects.ForEach(p =>
            {
                writer.WriteStartElement(p.Name);
                writer.WriteElementString("Path", p.Path);
                writer.WriteEndElement();

                if (!Directory.Exists(p.Path)) Directory.CreateDirectory(p.Path);

                XmlSerializer serializer = new XmlSerializer(typeof(Project));
                using (XmlWriter xmlWriter = XmlWriter.Create(p.Path + @"\" + p.Name + ".GameStory"))
                {
                    serializer.Serialize(xmlWriter, p);
                }
            });
            writer.WriteEndElement();
        }
    }

    public class ProjectCollection : List<Project>
    {
        public ProjectCollection() : base() { }
        public Project this[string Name]
        {
            get => base[FindIndex(match => match.Name == Name)];
            set => base[FindIndex(match => match.Name == Name)] = value;
        }
        public void Remove(string Name) => RemoveAt(FindIndex(match => match.Name == Name));
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

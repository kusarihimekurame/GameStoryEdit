using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    [Serializable]
    public class Project : ITreeItem, IXmlSerializable
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ObservableCollection<ITreeItem> Children { get; } = new ObservableCollection<ITreeItem>();
        public void Serialize()
        {
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (XmlWriter xmlWriter = XmlWriter.Create(Path + @"\" + Name + ".GameStory"))
            {
                serializer.Serialize(xmlWriter, this);
            }
        }
        public static Project Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (XmlReader Reader = XmlReader.Create(path))
            {
                return (Project)serializer.Deserialize(Reader);
            }
        }
        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                reader.MoveToContent();

                if (reader.LocalName.Equals("Project") && reader.NodeType == XmlNodeType.EndElement)
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
                    case "Children":
                        void _readxml(ITreeItem rootTreeItem)
                        {
                            while (reader.Read())
                            {
                                if (reader.LocalName.Equals("Children") && reader.NodeType == XmlNodeType.EndElement)
                                {
                                    break;
                                }

                                if (reader.NodeType == XmlNodeType.Element && !reader.LocalName.Equals("Name") && !reader.LocalName.Equals("Path") && !reader.LocalName.Equals("Children"))
                                {
                                    ITreeItem treeItem = (ITreeItem)Assembly.GetExecutingAssembly().CreateInstance("GameStoryEdit.TreeData." + reader.LocalName);
                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.EndElement && !reader.LocalName.Equals("Name") && !reader.LocalName.Equals("Path") && !reader.LocalName.Equals("Children"))
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
                                                treeItem.Name = reader.Value;
                                                break;
                                            case "Path":
                                                reader.Read();
                                                treeItem.Path = reader.Value;
                                                break;
                                            case "Children":
                                                _readxml(treeItem);
                                                break;
                                        }
                                    }
                                    rootTreeItem.Children.Add(treeItem);
                                }
                            }
                        }
                        _readxml(this);
                        break;
                }
            }
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Path", Path);

            writer.WriteStartElement("Children");
            void _writexml(ITreeItem rootTreeItem)
            {
                rootTreeItem.Children.ToList().ForEach(c =>
                {
                    writer.WriteStartElement(c.GetType().Name);
                    writer.WriteElementString("Name", c.Name);
                    writer.WriteElementString("Path", c.Path);
                    writer.WriteStartElement("Children");
                    _writexml(c);
                    writer.WriteEndElement();
                });
            }
            _writexml(this);
            writer.WriteEndElement();
        }
    }
}

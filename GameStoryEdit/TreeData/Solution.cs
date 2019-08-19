using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GameStoryEdit.TreeData
{
    [Serializable]
    public class Solution : IXmlSerializable
    {
        public ProjectCollection Projects { get; set; } = new ProjectCollection();
        public string Name { get; set; }
        public string Path { get; set; }
        public void Serialize()
        {
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            XmlSerializer serializer = new XmlSerializer(typeof(Solution));
            using (XmlWriter xmlWriter = XmlWriter.Create(Path + @"\" + Name + ".gse"))
            {
                serializer.Serialize(xmlWriter, this);
            }
        }
        public static Solution Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Solution));
            using (XmlReader Reader = XmlReader.Create(path))
            {
                return (Solution)serializer.Deserialize(Reader);
            }
        }
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

                                Projects.Add(Project.Deserialize(reader.Value + @"\" + name + ".GameStory"));
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

                p.Serialize();
            });
            writer.WriteEndElement();
        }
    }
}

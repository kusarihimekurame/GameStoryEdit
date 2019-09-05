using GameStoryEdit.Data;
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
        #region Data

        public ProjectCollection Projects { get; set; } = new ProjectCollection();
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension => ".gse";
        public string FileName => Name + Extension;
        public string FullName => Path + @"\" + Name + Extension;

        #endregion

        #region Serialize

        public void Serialize()
        {
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            XmlSerializer serializer = new XmlSerializer(typeof(Solution));
            using (XmlWriter xmlWriter = XmlWriter.Create(Path + @"\" + Name + ".gse"))
            {
                serializer.Serialize(xmlWriter, this);
            }

            HistoryCollection histories = HistoryCollection.Deserialize();
            if (histories.Contains(Name))
            {
                histories[Name].Extension = Extension;
                histories[Name].Path = Path;
                histories[Name].SaveTime = DateTime.Now;
            }
            else
            {
                histories.Add(new History() { Name = Name, Extension = Extension, Path = Path, SaveTime = DateTime.Now });
            }
            histories.Serialize();
        }

        #endregion

        #region Deserialize

        public static Solution Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Solution));
            using (XmlReader Reader = XmlReader.Create(path))
            {
                Solution solution = (Solution)serializer.Deserialize(Reader);

                HistoryCollection histories = HistoryCollection.Deserialize();
                if (histories.Contains(solution.Name))
                {
                    histories[solution.Name].Extension = solution.Extension;
                    histories[solution.Name].Path = solution.Path;
                    histories[solution.Name].OpenTime = DateTime.Now;
                }
                else
                {
                    histories.Add(new History() { Name = solution.Name, Extension = solution.Extension, Path = solution.Path, OpenTime = DateTime.Now });
                }
                histories.Serialize();

                return solution;
            }
        }

        #endregion

        #region XmlSerializable

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
                        string BasePath = reader.BaseURI.Substring(8, reader.BaseURI.Length - reader.BaseURI.Split('/').Last().Length - 9).Replace('/', '\\');
                        Path = Directory.Exists(reader.Value) ? reader.Value : BasePath;
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

                                Projects.Add(Project.Deserialize(Directory.Exists(reader.Value) ? reader.Value : Path.Substring(0, 3) + reader.Value.Substring(3) + @"\" + name + ".GameStory"));
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

        #endregion
    }
}

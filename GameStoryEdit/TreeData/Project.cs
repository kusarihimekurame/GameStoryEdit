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
    public class Project : BaseTreeItem, IXmlSerializable
    {
        #region Date

        public string Extension => ".GameStory";
        public string FileName => Name + Extension;
        public string FullName => Path + @"\" + Name + Extension;

        #endregion

        #region Serialize

        public void Serialize()
        {
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (XmlWriter xmlWriter = XmlWriter.Create(Path + @"\" + Name + ".GameStory"))
            {
                serializer.Serialize(xmlWriter, this);
            }
        }

        #endregion

        #region Deserialize

        public static Project Deserialize(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (XmlReader Reader = XmlReader.Create(path))
            {
                return (Project)serializer.Deserialize(Reader);
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
                        string BasePath = reader.BaseURI.Substring(8, reader.BaseURI.Length - reader.BaseURI.Split('/').Last().Length - 9).Replace('/', '\\');
                        Path = Directory.Exists(reader.Value) ? reader.Value : BasePath;
                        break;
                    case "Children":
                        void _readxml(BaseTreeItem rootTreeItem)
                        {
                            while (reader.Read())
                            {
                                if (reader.LocalName.Equals("Children") && reader.NodeType == XmlNodeType.EndElement)
                                {
                                    break;
                                }

                                if (reader.NodeType == XmlNodeType.Element && !reader.LocalName.Equals("Name") && !reader.LocalName.Equals("Path") && !reader.LocalName.Equals("Children"))
                                {
                                    BaseTreeItem treeItem = (BaseTreeItem)Assembly.GetExecutingAssembly().CreateInstance("GameStoryEdit.TreeData." + reader.LocalName);
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
                                                treeItem.Path = Directory.Exists(reader.Value) ? reader.Value : Path.Substring(0, 3) + reader.Value.Substring(3);
                                                break;
                                            case "Children":
                                                _readxml(treeItem);
                                                break;
                                        }
                                    }

                                    if (treeItem is ScreenPlay sp && sp.Name != null && sp.Path != null)
                                    {
                                        sp.FountainEditor = new UserControls.FountainEditor();
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
            void _writexml(BaseTreeItem rootTreeItem)
            {
                #region XML

                if (rootTreeItem != this) writer.WriteStartElement(rootTreeItem.GetType().Name);
                writer.WriteElementString("Name", rootTreeItem.Name);
                writer.WriteElementString("Path", rootTreeItem.Path);
                writer.WriteStartElement("Children");
                rootTreeItem.Children.ToList().ForEach(c => _writexml(c));
                writer.WriteEndElement();
                if (rootTreeItem != this) writer.WriteEndElement();

                #endregion

                #region Creat Directory and File

                if (rootTreeItem is Assets assets)
                {
                    if (!Directory.Exists(assets.Path)) Directory.CreateDirectory(assets.Path);
                    assets.Children.ToList().ForEach(a =>
                    {
                        if (!Directory.Exists(a.Path)) Directory.CreateDirectory(a.Path);
                    });
                }
                else if(rootTreeItem is ScreenPlay screenPlay)
                {
                    screenPlay.SaveText();
                }

                #endregion
            }
            _writexml(this);
        }

        #endregion
    }
}

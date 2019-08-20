using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GameStoryEdit.TreeData
{
    public class BaseTreeItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ChildrenCollection Children { get; } = new ChildrenCollection();
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStoryEdit.TreeData
{
    public class ChildrenCollection : ObservableCollection<BaseTreeItem>
    {
        public ChildrenCollection() : base() { }
        public BaseTreeItem this[string Name]
        {
            get => base[Items.IndexOf(Items.FirstOrDefault(match => match.Name == Name))];
            set => base[Items.IndexOf(Items.FirstOrDefault(match => match.Name == Name))] = value;
        }
        public void Remove(string Name) => RemoveAt(Items.IndexOf(Items.FirstOrDefault(match => match.Name == Name)));
    }
}

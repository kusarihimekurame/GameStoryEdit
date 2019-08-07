using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStoryEdit.TreeData
{
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
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string Name { get; set; }
        string Path { get; set; }
        ITreeItem Parent { get; }
        IList<ITreeItem> Children { get; }
    }
    public class Solution : ITreeItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ITreeItem Parent { get; } = null;
        public IList<ITreeItem> Children { get; } = new List<ITreeItem>();
    }
    public class Project : ITreeItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ITreeItem Parent { get; }
        public IList<ITreeItem> Children { get; } = new List<ITreeItem>();
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

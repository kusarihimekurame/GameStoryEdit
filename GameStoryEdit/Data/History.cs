using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameStoryEdit.Data
{
    [Serializable]
    [DebuggerDisplay("Name={Name},OpenTime={OpenTime}")]
    public class History
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string FileName => Name + Extension;
        public string FullName => Path + @"\" + Name + Extension;
        public DateTime OpenTime { get; set; }
        public DateTime SaveTime { get; set; }
    }
}

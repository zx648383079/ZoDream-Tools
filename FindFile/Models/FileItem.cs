using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Models
{
    public class FileItem
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime Mtime { get; set; }
        public DateTime Ctime { get; set; }

        public string Extension { get; set; }

        public string Md5 { get; set; }

        public string Crc32 { get; set; }

        public string FileName { get; set; }

        public string Guid { get; set; }

        public bool IsChecked { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.FindFile.ViewModels;

namespace ZoDream.FindFile.Models
{
    public class FilenNotifyItem: BindableBase
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


        private bool isChecked = false;

        public bool IsChecked
        {
            get => isChecked;
            set => Set(ref isChecked, value);
        }


        public FilenNotifyItem()
        {

        }

        public FilenNotifyItem(FileItem item)
        {
            Name = item.Name;
            Size = item.Size;
            Mtime = item.Mtime;
            Ctime = item.Ctime;
            FileName = item.FileName;
            Guid = item.Guid;
            Extension = item.Extension;
            Md5 = item.Md5;
            Crc32 = item.Crc32;
        }
    }
}

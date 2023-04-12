using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Models
{
    [Flags]
    public enum FileCompareFlags
    {
        None = 0,
        Name = 0x1,
        Size = 0x2,
        Mtime = 0x4,
        MD5 = 0x8,
        CRC32 = 0x10,
    }
}

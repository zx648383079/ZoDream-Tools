using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Models
{
    [Flags]
    public enum FileFilterFlags
    {
        None = 0,
        NotEmpty = 0x1,
        NotHidden = 0x2,
        NotLess = 0x4,
        FindImage = 0x10,
        FindMedia = 0x20,
        FindZip = 0x40,
        FindDoc = 0x8,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.FindFile.Models;

namespace ZoDream.FindFile.Shared
{
    public delegate void FinderLogEventHandler(string fileName);

    public delegate void FinderEventHandler(FileItem item);
    public delegate void FinderFinishedEventHandler();
}

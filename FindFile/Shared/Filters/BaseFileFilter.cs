using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Shared.Filters
{
    public abstract class BaseFileFilter : IFileFilter
    {
        public IList<FileInfo> Filter(IList<FileInfo> files)
        {
            return files.Where(i => Valid(i)).ToList();
        }

        public abstract bool Valid(FileInfo fileInfo);
    }
}

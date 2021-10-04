using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Shared.Filters
{
    public interface IFileFilter
    {

        public bool Valid(FileInfo fileInfo);

        public IList<FileInfo> Filter(IList<FileInfo> files);
    }
}

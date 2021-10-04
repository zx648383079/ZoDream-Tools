using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Shared.Filters
{
    public class EmptyFileFilter : BaseFileFilter
    {
        public EmptyFileFilter(bool ignore = false)
        {
            _ignore = ignore;
        }

        private bool _ignore;

        public override bool Valid(FileInfo fileInfo)
        {
            var res = fileInfo.Length <= 0;
            return _ignore ? !res : res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.FindFile.Shared.Filters
{
    public class SizeFileFilter : BaseFileFilter
    {
        public SizeFileFilter(long min, long max = 0)
        {
            _minLength = min;
            _maxLength = max;
        }

        private long _minLength;

        private long _maxLength;

        public override bool Valid(FileInfo fileInfo)
        {
            var len = fileInfo.Length;
            if (len < _minLength)
            {
                return false;
            }
            return _maxLength > 0 && len <= _maxLength;
        }
    }
}

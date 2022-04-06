using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.ControlBrowser.Comparers
{
    public class TypeComparer : IComparer<Type>
    {
        public int Compare(Type? x, Type? y)
        {
            return String.Compare(x?.Name, y?.Name, StringComparison.Ordinal);
        }
    }
}

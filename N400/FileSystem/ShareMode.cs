using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    public enum ShareMode : ushort
    {
        All,
        Write,
        Read,
        None
    }
}

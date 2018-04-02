using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    public enum OpenMode : ushort
    {
        Read = 1,
        Write,
        ReadWrite,
        ProgramLoad
    }
}

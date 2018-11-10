using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data.Internals
{
    internal enum SqlFreeStmtOptions : short
    {
        Close = 0,
        Drop = 1,
        Unbind = 2,
        ResetParams = 3
    }
}

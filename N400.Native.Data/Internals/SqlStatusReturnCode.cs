using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data.Internals
{
    internal enum SqlStatusReturnCode : int
    {
        Success = 0,
        SuccessWithInfo = 1,
        StillExecuting = 2,
        Error = -1,
        InvalidHandle = -2,
        NoData = 100
    }
}

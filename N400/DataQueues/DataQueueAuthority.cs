using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.DataQueues
{
    public enum DataQueueAuthority : byte
    {
        LibCrtAut = 0xF4,
        All = 0xF0,
        Change = 0xF1,
        Exclude = 0xF2,
        Other = 0xF3
    }
}

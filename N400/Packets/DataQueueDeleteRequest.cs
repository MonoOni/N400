using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueDeleteRequest : DataQueueRequestBase
    {
        const ushort ID = 0x0004;

        public DataQueueDeleteRequest(byte[] name, byte[] library)
            : base(40, name, library)
        {
            RequestResponseID = ID;
        }
    }
}

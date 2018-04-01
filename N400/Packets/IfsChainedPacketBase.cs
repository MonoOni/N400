using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal abstract class IfsChainedPacketBase : Packet
    {
        public ushort Chain
        {
            get
            {
                return Data.ReadUInt16BE(20);
            }
            set
            {
                Data.WriteBE(20, value);
            }
        }

        public IfsChainedPacketBase(int length) : base(length)
        {
            ServiceID = IfsService.SERVICE_ID;
        }

        public IfsChainedPacketBase(byte[] data) : base(data)
        {
        }
    }
}

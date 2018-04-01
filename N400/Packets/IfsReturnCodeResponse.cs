using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsReturnCodeResponse : Packet
    {
        public const ushort ID = 0x8001;

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

        public ushort ReturnCode
        {
            get
            {
                return Data.ReadUInt16BE(22);
            }
            set
            {
                Data.WriteBE(22, value);
            }
        }

        public IfsReturnCodeResponse(byte[] data)
            : base(data)
        {
        }
    }
}

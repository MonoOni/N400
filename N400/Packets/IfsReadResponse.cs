using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsReadResponse : IfsChainedPacketBase
    {
        public const ushort ID = 0x8003;

        public ushort CCSID => Data.ReadUInt16BE(22);

        // address this more directly

        public int FieldLength => Data.ReadInt32BE(24);

        public byte[] FieldData => Data.Slice(30, FieldLength - 6);

        public IfsReadResponse(byte[] data) : base(data)
        {
        }
    }
}

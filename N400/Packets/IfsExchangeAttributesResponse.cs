using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsExchangeAttributesResponse : Packet
    {
        public ushort ReturnCode => Data.ReadUInt16BE(20);

        public ushort DataStreamLevel => Data.ReadUInt16BE(22);

        public uint MaxDataBlock => Data.ReadUInt32BE(26);

        public IEnumerable<ushort> PreferredCCSID
        {
            get
            {
                var items = GetField(0x000A);
                // this length excludes the int for length and ushort id
                if (items.Length % sizeof(ushort) != 0)
                    throw new Exception("The array isn't the right size.");

                var list = new List<ushort>();
                var offset = 0;
                while (items.Length > offset)
                {
                    var item = items.ReadUInt16BE(offset);
                    list.Add(item);
                    offset += sizeof(ushort);
                }
                return list;
            }
        }

        public IfsExchangeAttributesResponse() : base()
        {
            throw new NotImplementedException();
        }

        public IfsExchangeAttributesResponse(byte[] data) : base(data)
        {
        }
    }
}

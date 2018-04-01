using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsExchangeAttributesResponse : Packet
    {
        public ushort ReturnCode
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

        public ushort DataStreamLevel
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

        public uint MaxDataBlock
        {
            get
            {
                return Data.ReadUInt32BE(26);
            }
            set
            {
                Data.WriteBE(26, value);
            }
        }

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
            set
            {
                if (value != null)
                {
                    var count = value.Count() * sizeof(ushort);
                    var bytes = new byte[count];
                    var offset = 0;
                    var index = 0;
                    while (count > offset)
                    {
                        bytes.WriteBE(offset, value.ElementAt(index++));
                        offset += sizeof(ushort);
                    }
                    SetField(bytes, 30, 0x000A);
                }
                else
                {
                    SetField(null, 30, 0x000A);
                }
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

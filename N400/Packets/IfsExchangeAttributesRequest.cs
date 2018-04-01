using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsExchangeAttributesRequest : IfsChainedPacketBase
    {
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

        // TODO: convert this to an enum, if a [Flags] enum could work
        /// <remarks>
        /// Supported options:
        /// <list type="bullet">
        /// <item>1: POSIX globbing</item>
        /// <item>2: POSIX_ALL globbing</item>
        /// <item>3: OS/2 globbing</item>
        /// <item>4: GMT</item>
        /// <item>8: POSIX return codes</item>
        /// </list>
        /// </remarks>
        public ushort Flags
        {
            get
            {
                return Data.ReadUInt16BE(24);
            }
            set
            {
                Data.WriteBE(24, value);
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
        
        // TODO: make this adjustable; maybe a fallback to unicode CCSIDs?
        public IfsExchangeAttributesRequest(ushort[] ccsids)
            : base(36 + ((ccsids?.Length ?? 0) * 8))
        {
            RequestResponseID = 0x0016;
            TemplateLength = 10;

            Chain = 0;
            DataStreamLevel = 8;
            Flags = 6;
            PreferredCCSID = ccsids;
        }
    }
}

using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsReadRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x0003;

        public uint Handle
        {
            get
            {
                return Data.ReadUInt32BE(22);
            }
            set
            {
                Data.WriteBE(22, value);
            }
        }

        // two uints for 32-bit offset

        public int ReadLength
        {
            get
            {
                return Data.ReadInt32BE(34);
            }
            set
            {
                Data.WriteBE(34, value);
            }
        }

        public int PreReadLength
        {
            get
            {
                return Data.ReadInt32BE(38);
            }
            set
            {
                Data.WriteBE(38, value);
            }
        }

        public long BaseOffset
        {
            get
            {
                return Data.ReadInt64BE(42);
            }
            set
            {
                Data.WriteBE(42, value);
            }
        }

        public long RelativeOffset
        {
            get
            {
                return Data.ReadInt64BE(50);
            }
            set
            {
                Data.WriteBE(50, value);
            }
        }

        public IfsReadRequest(uint handle, long offset, int length)
            : base(58)
        {
            RequestResponseID = ID;
            TemplateLength = 38;

            Handle = handle;
            ReadLength = length;
            PreReadLength = 0;
            BaseOffset = 0;
            RelativeOffset = offset;
        }
    }
}

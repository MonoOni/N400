using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsWriteRequest : IfsChainedPacketBase
    {
        public const ushort ID = 0x0004;

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

        public bool Sync
        {
            get
            {
                return Data.ReadUInt16BE(34) == 3;
            }
            set
            {
                Data.WriteBE(34, (ushort)(value ? 3 : 2));
            }
        }

        public ushort CCSID
        {
            get
            {
                return Data.ReadUInt16BE(36);
            }
            set
            {
                Data.WriteBE(36, value);
            }
        }

        public long BaseOffset
        {
            get
            {
                return Data.ReadInt64BE(38);
            }
            set
            {
                Data.WriteBE(38, value);
            }
        }

        public long RelativeOffset
        {
            get
            {
                return Data.ReadInt64BE(46);
            }
            set
            {
                Data.WriteBE(46, value);
            }
        }

        public byte[] ToWrite
        {
            get
            {
                return GetField(0x0020);
            }
            set
            {
                SetField(value, 54, 0x0020);
            }
        }

        public IfsWriteRequest(uint handle, byte[] data, long offset, bool sync, ushort ccsid)
            : base(60 + data.Length)
        {
            TemplateLength = 34;
            RequestResponseID = ID;

            Handle = handle;
            Sync = sync;
            CCSID = ccsid;
            BaseOffset = offset;
            RelativeOffset = 0;
            ToWrite = data;
        }
    }
}

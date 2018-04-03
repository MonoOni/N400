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

        // HACK: what the fuck is going on jt400? if we used the extended long
        // version of the packet (left commented) it just..... seems to ignore
        // both 32 and 64-bit values for offset? I don't know if
        // jt400/jtopenlite is broken, or if the as/400 ifs protocol is... i
        // need to investigate this one further

        public long BaseOffset
        {
            get
            {
                return Data.ReadInt32BE(26);
                //return Data.ReadInt64BE(38);
            }
            set
            {
                Data.WriteBE(26, Convert.ToInt32(value));
                //Data.WriteBE(38, value);
            }
        }

        public long RelativeOffset
        {
            get
            {
                return Data.ReadInt32BE(30);
                //return Data.ReadInt64BE(46);
            }
            set
            {
                Data.WriteBE(30, Convert.ToInt32(value));
                //Data.WriteBE(46, value);
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
                //SetField(value, 54, 0x0020);
                SetField(value, 38, 0x0020);
            }
        }

        // if in long mode it's 60 + data.Length and a 34 template length
        public IfsWriteRequest(uint handle, byte[] data, long offset, bool sync, ushort ccsid)
            : base(26 + 18 + data.Length)
        {
            TemplateLength = 18;
            RequestResponseID = ID;

            Handle = handle;
            Sync = sync;
            CCSID = ccsid;
            BaseOffset = 0;
            RelativeOffset = offset;
            ToWrite = data;
        }
    }
}

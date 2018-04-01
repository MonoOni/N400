using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsListAttributesRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x000A;

        public uint FileHandle
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

        public ushort CCSID
        {
            get
            {
                return Data.ReadUInt16BE(26);
            }
            set
            {
                Data.WriteBE(26, value);
            }
        }

        public uint WorkingDirectoryHandle
        {
            get
            {
                return Data.ReadUInt32BE(28);
            }
            set
            {
                Data.WriteBE(28, value);
            }
        }

        public ushort Authority
        {
            get
            {
                return Data.ReadUInt16BE(32);
            }
            set
            {
                Data.WriteBE(32, value);
            }
        }

        public ushort MaxItems
        {
            get
            {
                return Data.ReadUInt16BE(34);
            }
            set
            {
                Data.WriteBE(34, value);
            }
        }

        public ushort AttributeListLevel
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

        // TODO: convert to enum
        public ushort PatternMatchingMode
        {
            get
            {
                return Data.ReadUInt16BE(38);
            }
            set
            {
                Data.WriteBE(38, value);
            }
        }

        public byte[] Path
        {
            get
            {
                return GetField(0x0002);
            }
            set
            {
                SetField(value ?? new byte[] { }, 40, 0x0002);
            }
        }

        // TODO: make this customizable
        public IfsListAttributesRequest(byte[] pathBytes)
            : base(46 + pathBytes.Length)
        {
            TemplateLength = 20;
            RequestResponseID = ID;

            Chain = 0;
            FileHandle = 0;
            CCSID = 1200; // IfsService hardcoded for 1200
            WorkingDirectoryHandle = 1;
            Authority = 0;
            MaxItems = ushort.MaxValue;
            AttributeListLevel = 0x0101; // returns a ulong filesize
            PatternMatchingMode = 1; // POSIX all
            Path = pathBytes;
        }
    }
}

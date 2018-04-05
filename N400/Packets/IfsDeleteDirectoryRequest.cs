using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsDeleteDirectoryRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x000E;

        public ushort FileNameCCSID
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

        public uint WorkingDirectoryHandle
        {
            get
            {
                return Data.ReadUInt32BE(24);
            }
            set
            {
                Data.WriteBE(24, value);
            }
        }

        public ushort Flags
        {
            get
            {
                return Data.ReadUInt16BE(28);
            }
            set
            {
                Data.WriteBE(28, value);
            }
        }

        public byte[] FileName
        {
            get
            {
                return GetField(0x0001);
            }
            set
            {
                SetField(value, 30, 0x0001);
            }
        }

        public IfsDeleteDirectoryRequest(byte[] fileName)
            : base(36 + fileName.Length)
        {
            RequestResponseID = ID;
            TemplateLength = 10;

            FileNameCCSID = 1200; // unhardcode?
            WorkingDirectoryHandle = 1;
            FileName = fileName;
            Flags = 0;
        }
    }
}

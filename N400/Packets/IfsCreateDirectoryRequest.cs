using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsCreateDirectoryRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x000D;

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

        public byte[] FileName
        {
            get
            {
                return GetField(0x0001);
            }
            set
            {
                SetField(value, 28, 0x0001);
            }
        }

        public IfsCreateDirectoryRequest(byte[] fileName)
            : base(34 + fileName.Length)
        {
            RequestResponseID = ID;
            TemplateLength = 8;

            FileNameCCSID = 1200; // unhardcode?
            WorkingDirectoryHandle = 1;
            FileName = fileName;
        }
    }
}

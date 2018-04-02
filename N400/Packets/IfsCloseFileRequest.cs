using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsCloseFileRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x0009;

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

        public ushort Flags
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

        public ushort CCSID
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

        public ushort AmountAccessed
        {
            get
            {
                return Data.ReadUInt16BE(30);
            }
            set
            {
                Data.WriteBE(30, value);
            }
        }

        public byte AccessHistory
        {
            get
            {
                return Data[32];
            }
            set
            {
                Data[32] = value;
            }
        }

        public ulong ModificationDate
        {
            get
            {
                // we dont need to set this manually, service does it
                // so use 0 instead

                //return Data.ReadDateTimeIfs(33);
                return Data.ReadUInt64BE(33);
            }
            set
            {
                Data.WriteBE(33, value);
            }
        }

        public IfsCloseFileRequest(uint handle) : base(41)
        {
            ServiceID = IfsService.SERVICE_ID;
            RequestResponseID = ID;
            TemplateLength = 21;

            Handle = handle;
            Flags = 2;
            CCSID = ushort.MaxValue;
            AmountAccessed = 100;
            AccessHistory = 0;
            ModificationDate = 0;
        }
    }
}

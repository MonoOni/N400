using N400.DataQueues;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueCreateRequest : DataQueueRequestBase
    {
        const ushort ID = 0x0003;

        public uint EntryLength
        {
            get
            {
                return Data.ReadUInt32BE(40);
            }
            set
            {
                Data.WriteBE(40, value);
            }
        }

        public DataQueueAuthority Authority
        {
            get
            {
                return (DataQueueAuthority)Data[44];
            }
            set
            {
                Data[44] = (byte)value;
            }
        }

        public bool SaveSenderInfo
        {
            get
            {
                return Data[45] == 0xF1;
            }
            set
            {
                Data[45] = (byte)(value ? 0xF1 : 0xF0);
            }
        }

        public byte Type
        {
            get
            {
                return Data[46];
            }
            set
            {
                Data[46] = value;
            }
        }

        public uint KeyLength
        {
            get
            {
                return Data.ReadUInt16BE(47);
            }
            set
            {
                Data.WriteBE(47, value);
            }
        }

        public bool ForceStorage
        {
            get
            {
                return Data[49] == 0xF1;
            }
            set
            {
                Data[49] = (byte)(value ? 0xF1 : 0xF0);
            }
        }

        public byte[] Description
        {
            get
            {
                return Data.Slice(50, 50);
            }
            set
            {
                Array.Copy(value, 0, Data, 50, Math.Min(value.Length, 50));
            }
        }

        public DataQueueCreateRequest(byte[] name,
            byte[] library,
            uint entryLength,
            bool saveSenderInfo,
            bool fifo,
            DataQueueAuthority authority,
            uint keyLength,
            bool forceStorage,
            byte[] description) : base(100, name, library)
        {
            TemplateLength = 80;
            RequestResponseID = ID;
            
            Description = description;
            EntryLength = entryLength;
            Authority = authority;
            SaveSenderInfo = saveSenderInfo;
            KeyLength = keyLength;
            ForceStorage = ForceStorage;

            if (keyLength == 0)
            {
                Type = (byte)(fifo ? 0xF0 : 0xF1);
            }
            else
            {
                Type = 0xF2;
            }
        }
    }
}

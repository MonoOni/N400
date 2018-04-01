using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsListAttributeResponse : IfsChainedPacketBase
    {
        public const ushort ID = 0x8005;

        public DateTime CreationDate
        {
            get
            {
                return Data.ReadDateTimeIfs(22);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ModificationDate
        {
            get
            {
                return Data.ReadDateTimeIfs(30);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime AccessDate
        {
            get
            {
                return Data.ReadDateTimeIfs(38);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public uint FixedAttributes
        {
            get
            {
                return Data.ReadUInt32BE(50);
            }
            set
            {
                Data.WriteBE(50, value);
            }
        }

        public ushort ObjectType
        {
            get
            {
                return Data.ReadUInt16BE(54);
            }
            set
            {
                Data.WriteBE(54, value);
            }
        }

        public ushort ExtendedAttributesCount
        {
            get
            {
                return Data.ReadUInt16BE(56);
            }
            set
            {
                Data.WriteBE(56, value);
            }
        }

        public uint ExtendedAttributeNamesBytes
        {
            get
            {
                return Data.ReadUInt32BE(58);
            }
            set
            {
                Data.WriteBE(58, value);
            }
        }

        public uint ExtendedAttributeValuesBytes
        {
            get
            {
                return Data.ReadUInt32BE(62);
            }
            set
            {
                Data.WriteBE(62, value);
            }
        }

        public uint Version
        {
            get
            {
                return Data.ReadUInt32BE(66);
            }
            set
            {
                Data.WriteBE(66, value);
            }
        }

        public ushort AmountAccessed
        {
            get
            {
                return Data.ReadUInt16BE(70);
            }
            set
            {
                Data.WriteBE(70, value);
            }
        }

        public byte AccessHistory
        {
            get
            {
                return Data[72];
            }
            set
            {
                Data[72] = value;
            }
        }

        public ushort FileCCSID
        {
            get
            {
                return Data.ReadUInt16BE(73);
            }
            set
            {
                Data.WriteBE(73, value);
            }
        }

        public ushort CheckoutCCSID
        {
            get
            {
                return Data.ReadUInt16BE(75);
            }
            set
            {
                Data.WriteBE(75, value);
            }
        }

        public uint RestartID
        {
            get
            {
                return Data.ReadUInt32BE(88);
            }
            set
            {
                Data.WriteBE(88, value);
            }
        }

        public ulong FileSize
        {
            get
            {
                return Data.ReadUInt64BE(81);
            }
            set
            {
                Data.WriteBE(81, value);
            }
        }

        // mystery meat ushort at 89

        public bool Symlink
        {
            get
            {
                return Data[91] == 1;
            }
            set
            {
                Data[91] = (byte)(value ? 1 : 0);
            }
        }

        public byte[] FileName
        {
            get
            {
                return GetField(0x0002);
            }
        }

        public IfsListAttributeResponse(byte[] data)
            : base(data)
        {
        }
    }
}

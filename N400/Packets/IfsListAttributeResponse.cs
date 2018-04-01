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

        public DateTime CreationDate => Data.ReadDateTimeIfs(22);

        public DateTime ModificationDate => Data.ReadDateTimeIfs(30);

        public DateTime AccessDate => Data.ReadDateTimeIfs(38);

        // file size uint at 46, but obsoleted by the ulong version

        public uint FixedAttributes => Data.ReadUInt32BE(50);

        public ushort ObjectType => Data.ReadUInt16BE(54);

        public ushort ExtendedAttributesCount => Data.ReadUInt16BE(56);

        public uint ExtendedAttributeNamesBytes => Data.ReadUInt32BE(58);

        public uint ExtendedAttributeValuesBytes => Data.ReadUInt32BE(62);

        public uint Version => Data.ReadUInt32BE(66);

        public ushort AmountAccessed => Data.ReadUInt16BE(70);

        public byte AccessHistory => Data[72];

        public ushort FileCCSID => Data.ReadUInt16BE(73);

        public ushort CheckoutCCSID => Data.ReadUInt16BE(75);

        public uint RestartID => Data.ReadUInt32BE(88);

        public ulong FileSize => Data.ReadUInt64BE(81);

        // mystery meat ushort at 89

        public bool Symlink => Data[91] == 1;

        public byte[] FileName => GetField(0x0002);

        public IfsListAttributeResponse(byte[] data)
            : base(data)
        {
        }
    }
}

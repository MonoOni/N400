using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsOpenFileResponse : IfsChainedPacketBase
    {
        public const ushort ID = 0x8002;

        public uint Handle => Data.ReadUInt32BE(22);

        public ulong HandleId => Data.ReadUInt16BE(26);

        public ushort FileCCSID => Data.ReadUInt16BE(34);

        public ushort Action => Data.ReadUInt16BE(36);

        public DateTime CreationDate => Data.ReadDateTimeIfs(38);

        public DateTime ModificationDate => Data.ReadDateTimeIfs(46);

        public DateTime AccessDate => Data.ReadDateTimeIfs(54);

        // another uint file size at 62....

        public uint FixedAttributes => Data.ReadUInt32BE(66);

        public ushort NeedExtendedAttributes => Data.ReadUInt16BE(70);

        public ushort ExtendedAttributesCount => Data.ReadUInt16BE(72);
        
        public uint ExtendedAttributeNamesChars => Data.ReadUInt32BE(74);

        public uint ExtendedAttributeValuesBytes => Data.ReadUInt32BE(78);

        public uint Version => Data.ReadUInt32BE(82);

        public ushort AmountAccessed => Data.ReadUInt16BE(86);

        public byte AccessHistory => Data[88];

        public ulong FileSize => Data.ReadUInt64BE(89);

        public IfsOpenFileResponse(byte[] data) : base(data)
        {
        }
    }
}

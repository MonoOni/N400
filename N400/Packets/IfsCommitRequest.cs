using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsCommitRequest : IfsChainedPacketBase
    {
        public const ushort ID = 0x0006;

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

        public IfsCommitRequest(uint handle) : base(26)
        {
            TemplateLength = 26;
            RequestResponseID = ID;

            Handle = handle;
        }
    }
}

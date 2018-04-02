using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsWriteResponse : IfsReturnCodeResponse
    {
        public new const ushort ID = 0x800B;

        public int PreviousFileSize => Data.ReadInt32BE(24);

        public int BytesNotWritten => Data.ReadInt32BE(28);

        public IfsWriteResponse(byte[] data) : base(data)
        {
        }
    }
}

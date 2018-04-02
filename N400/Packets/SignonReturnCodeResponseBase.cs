using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal abstract class SignonReturnCodeResponseBase : Packet
    {
        public uint ReturnCode => Data.ReadUInt32BE(20);
        
        public SignonReturnCodeResponseBase() : this(24)
        {
        }
        
        public SignonReturnCodeResponseBase(int size) : base(size)
        {
            TemplateLength = 4;
        }
        
        public SignonReturnCodeResponseBase(byte[] data) : base(data)
        {
        }
    }
}

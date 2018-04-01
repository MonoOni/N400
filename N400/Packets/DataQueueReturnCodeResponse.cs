using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueReturnCodeResponse : Packet
    {
        public const ushort ID = 0x8002;

        // this is a ushort, so we can't inherit from ResponsePacket
        public ushort ReturnCode => Data.ReadUInt16BE(20);

        public byte[] Message
        {
            get
            {
                if (PacketLength > 22)
                {
                    return GetFields().SingleOrDefault().Value;
                }
                else return null;
            }
        }

        public DataQueueReturnCodeResponse(byte[] data) : base(data)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    /// <summary>
    /// Represents a response packet containing a return code, amongst other
    /// possible data.
    /// </summary>
    /// <remarks>
    /// This is mostly useful as a class to inherit; regardless, it is left
    /// non-abstract to allow use for boxing as a base class and to use packets
    /// not yet implemented.
    /// </remarks>
    public class ResponsePacket : Packet
    {
        /// <summary>
        /// The error code of the response. Usually, 0 is success.
        /// </summary>
        public uint ReturnCode
        {
            get
            {
                return Data.ReadUInt32BE(20);
            }
            set
            {
                Data.WriteBE(20, value);
            }
        }

        /// <summary>
        /// Creates an empty packet as long as the header and return code.
        /// </summary>
        public ResponsePacket() : this(24)
        {
        }

        /// <summary>
        /// Creates an empty packet of a certain size.
        /// </summary>
        /// <param name="size">
        /// How big the packet is, in bytes, including the length fields.
        /// </param>
        public ResponsePacket(int size) : base(size)
        {
            TemplateLength = 4;
        }

        /// <summary>
        /// Creates a packet from existing conttents.
        /// </summary>
        /// <param name="data">The byte array containing the packet.</param>
        public ResponsePacket(byte[] data) : base(data)
        {
        }
    }
}

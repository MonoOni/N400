using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    /// <summary>
    /// Represents a packet sent or recieved to or from a service.
    /// </summary>
    /// <remarks>
    /// This is mostly useful as a class to inherit; regardless, it is left
    /// non-abstract to allow use for boxing as a base class and to use packets
    /// not yet implemented.
    /// </remarks>
    public class Packet
    {
        /// <summary>
        /// The raw contents of the packet.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// The length of the packet. This should be consistent with
        /// <see cref="Data"/>'s length.
        /// </summary>
        public int PacketLength
        {
            get
            {
                return Data.ReadInt32BE(0);
            }
            protected set
            {
                Data.WriteBE(0, value);
            }
        }

        /// <summary>
        /// The ID of the service that the packet belongs to.
        /// </summary>
        public ushort ServiceID
        {
            get
            {
                return Data.ReadUInt16BE(6);
            }
            protected set
            {
                Data.WriteBE(6, value);
            }
        }

        /// <summary>
        /// An identifier used to identify a stream of packets with.
        /// </summary>
        public uint CorrelationID
        {
            get
            {
                return Data.ReadUInt32BE(12);
            }
            set
            {
                Data.WriteBE(12, value);
            }
        }

        /// <summary>
        /// How many bytes until field-based data can be placed, after the
        /// initial header shared by all packets.
        /// </summary>
        /// <remarks>
        /// Field-based data means fields that are addressed with
        /// <see cref="GetField(ushort)"/> instead of raw access.
        /// </remarks>
        public ushort TemplateLength
        {
            get
            {
                return Data.ReadUInt16BE(16);
            }
            protected set
            {
                Data.WriteBE(16, value);
            }
        }

        /// <summary>
        /// The ID of the packet's type.
        /// </summary>
        public ushort RequestResponseID
        {
            get
            {
                return Data.ReadUInt16BE(18);
            }
            protected set
            {
                Data.WriteBE(18, value);
            }
        }

        /// <summary>
        /// Creates an empty packet that is as long as the header.
        /// </summary>
        public Packet() : this(20)
        {
        }

        /// <summary>
        /// Creates an empty packet of a certain size.
        /// </summary>
        /// <param name="size">
        /// How big the packet is, in bytes, including the length fields.
        /// </param>
        public Packet(int size)
        {
            Data = new byte[size];
            PacketLength = Data.Length;
        }

        /// <summary>
        /// Creates a packet from existing conttents.
        /// </summary>
        /// <param name="data">The byte array containing the packet.</param>
        public Packet(byte[] data) : this(data.Length)
        {
            Array.Copy(data, Data, data.Length);
        }

        /// <summary>
        /// Gets all the fields in the packet.
        /// </summary>
        /// <returns>A dictionary that maps IDs to data.</returns>
        public Dictionary<ushort, byte[]> GetFields()
        {
            var dict = new Dictionary<ushort, byte[]>();

            var offset = TemplateLength + 20;
            var prevOffset = offset;
            var length = 0;
            while (offset < Data.Length - 1)
            {
                prevOffset = offset;
                length = Data.ReadInt32BE(offset);

                var currentId = Data.ReadUInt16BE(offset + sizeof(int));
                var start = offset + sizeof(int) + sizeof(short);
                dict.Add(currentId, Data.Slice(start, length - 6));

                offset += length;
                if (offset == prevOffset)
                    offset = Data.Length;
            }
            return dict;
        }

        /// <summary>
        /// Gets the raw content of a field by its ID.
        /// </summary>
        /// <param name="id">The ID of the field to get.</param>
        /// <returns>
        /// <para>
        /// A byte array containing the contents of the field. This is
        /// big-endian data, and can be accessed with functions like in
        /// <see cref="BitConverter"/> or <see cref="StreamExtensions"/>.
        /// </para>
        /// <para>
        /// If the field couldn't be found, then null will be returned.
        /// </para>
        /// </returns>
        protected byte[] GetField(ushort id)
        {
            var offset = TemplateLength + 20;
            var prevOffset = offset;
            var length = 0;
            while (offset < Data.Length - 1)
            {
                prevOffset = offset;
                length = Data.ReadInt32BE(offset);
                if (Data.ReadUInt16BE(offset + sizeof(int)) == id)
                    break;
                offset += length;
                if (offset == prevOffset)
                    offset = Data.Length;
            }
            if (offset >= Data.Length)
                return null;

            // is this right?
            var start = offset + sizeof(int) + sizeof(short);
            return Data.Slice(start, length - 6);
        }

        /// <summary>
        /// Sets the contents of a field.
        /// </summary>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="offset">The position of the field in the stream.</param>
        /// <param name="id">
        /// The ID of the field, used for addressing with
        /// <see cref="GetField(ushort)"/>.
        /// </param>
        protected void SetField(byte[] value, int offset, ushort id)
        {
            Data.WriteBE(offset, value.Length + 6);
            Data.WriteBE(offset + sizeof(int), id);
            Array.Copy(value, 0, Data, offset + sizeof(int) + sizeof(short), value.Length);
        }

        /// <summary>
        /// Sets the contents of a field.
        /// </summary>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="offset">The position of the field in the stream.</param>
        /// <param name="id">
        /// The ID of the field, used for addressing with
        /// <see cref="GetField(ushort)"/>.
        /// </param>
        protected void SetField(uint value, int offset, ushort id)
        {
            Data.WriteBE(offset, sizeof(uint) + 6);
            Data.WriteBE(offset + sizeof(int), id);
            Data.WriteBE(offset + sizeof(int) + sizeof(short), value);
        }

        /// <summary>
        /// Sets the contents of a field.
        /// </summary>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="offset">The position of the field in the stream.</param>
        /// <param name="id">
        /// The ID of the field, used for addressing with
        /// <see cref="GetField(ushort)"/>.
        /// </param>
        protected void SetField(ushort value, int offset, ushort id)
        {
            Data.WriteBE(offset, sizeof(ushort) + 6);
            Data.WriteBE(offset + sizeof(int), id);
            Data.WriteBE(offset + sizeof(int) + sizeof(short), value);
        }

        /// <summary>
        /// Sets the contents of a field.
        /// </summary>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="offset">The position of the field in the stream.</param>
        /// <param name="id">
        /// The ID of the field, used for addressing with
        /// <see cref="GetField(ushort)"/>.
        /// </param>
        protected void SetField(DateTime value, int offset, ushort id)
        {
            Data.WriteBE(offset, 8 + 6);
            Data.WriteBE(offset + sizeof(int), id);
            Data.WriteBE(offset + 6, value.Year);
            Data[offset + 6 + 1] = (byte)value.Month;
            Data[offset + 6 + 2] = (byte)value.Day;
            Data[offset + 6 + 3] = (byte)value.Hour;
            Data[offset + 6 + 4] = (byte)value.Minute;
            Data[offset + 6 + 5] = (byte)value.Second;
            // one padding byte
        }

        /// <summary>
        /// Reads a packet from the stream and returns it.
        /// </summary>
        /// <typeparam name="T">
        /// The type of packet to receive and parse.
        /// </typeparam>
        /// <param name="connection">The stream to read from.</param>
        /// <returns>The packet in the stream.</returns>
        /// <remarks>
        /// Note that in error cases, the packet may be smaller than expected,
        /// and as such, are not a full packet. These generally have return
        /// codes, so check the return code before accessing any members not
        /// found in child classes.
        /// </remarks>
        internal static T ReadPacket<T>(Stream connection) where T : Packet, new()
        {
            // read the length field, then the rest
            var buf = new byte[4];
            connection.Read(buf, 0, 4);
            var len = buf.ReadInt32BE();
            Array.Resize(ref buf, len);
            connection.Read(buf, 4, len - 4);
            return Activator.CreateInstance(typeof(T), new object[] { buf }) as T;
        }

        /// <summary>
        /// Writes a packet to the stream.
        /// </summary>
        /// <param name="connection">The stream to write to.</param>
        /// <param name="p">The packet to write</param>
        internal static void WritePacket(Stream connection, Packet p)
        {
            connection.Write(p.Data, 0, p.Data.Length);
            connection.Flush();
        }
    }
}

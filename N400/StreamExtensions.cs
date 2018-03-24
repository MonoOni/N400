using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400
{
    /// <summary>
    /// Utilitity library of functions to make consuming and producing data in
    /// streams simpler, including tasks like transparent endian conversion.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Creates a new array from a section of an existing array.
        /// </summary>
        /// <param name="source">The array to slice.</param>
        /// <param name="offset">The beggining of the section to copy.</param>
        /// <param name="length">How much to copy.</param>
        /// <returns>The new array.</returns>
        public static byte[] Slice(this byte[] source, int offset, int length)
        {
            var dest = new byte[length];
            Array.Copy(source, offset, dest, 0, length);
            return dest;
        }

        // TODO: Could these and Packet.GetField be converted to <T> variants?

        /// <summary>
        /// Reads 2 bytes in big endian and returns a signed 16-bit integer.
        /// </summary>
        /// <param name="br">
        /// A <see cref="BinaryReader"/> that has a short in big endian to read.
        /// </param>
        /// <returns>A signed 16-bit integer.</returns>
        public static short ReadInt16BE(this BinaryReader br)
        {
            var b = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt16(b, 0);
        }

        /// <summary>
        /// Reads 2 bytes in big endian and returns a signed 16-bit integer.
        /// </summary>
        /// <param name="ba">
        /// A byte array that has a short in big endian to read.
        /// </param>
        /// <param name="offset">The position of the data.</param>
        /// <returns>A signed 16-bit integer.</returns>
        public static short ReadInt16BE(this byte[] ba, int offset = 0)
        {
            var b = new byte[2];
            Array.Copy(ba, offset, b, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt16(b, 0);
        }

        /// <summary>
        /// Writes a value to an array in big endian.
        /// </summary>
        /// <param name="ba">The array to write to.</param>
        /// <param name="offset">The starting position to write to.</param>
        /// <param name="value">The signed 16-bit integer to write.</param>
        public static void WriteBE(this byte[] ba, int offset, short value)
        {
            var b = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            Array.Copy(b, 0, ba, offset, b.Length);
        }
        /// <summary>
        /// Reads 2 bytes in big endian and returns an usigned 16-bit integer.
        /// </summary>
        /// <param name="br">
        /// A <see cref="BinaryReader"/> that has a short in big endian to read.
        /// </param>
        /// <returns>An unsigned 16-bit integer.</returns>
        public static ushort ReadUInt16BE(this BinaryReader br)
        {
            var b = br.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToUInt16(b, 0);
        }

        /// <summary>
        /// Reads 2 bytes in big endian and returns a signed 16-bit integer.
        /// </summary>
        /// <param name="ba">
        /// A byte array that has a short in big endian to read.
        /// </param>
        /// <param name="offset">The position of the data.</param>
        /// <returns>A signed 16-bit integer.</returns>
        public static ushort ReadUInt16BE(this byte[] ba, int offset = 0)
        {
            var b = new byte[2];
            Array.Copy(ba, offset, b, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToUInt16(b, 0);
        }

        /// <summary>
        /// Writes a value to an array in big endian.
        /// </summary>
        /// <param name="ba">The array to write to.</param>
        /// <param name="offset">The starting position to write to.</param>
        /// <param name="value">The unsigned 16-bit integer to write.</param>
        public static void WriteBE(this byte[] ba, int offset, ushort value)
        {
            var b = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            Array.Copy(b, 0, ba, offset, b.Length);
        }

        /// <summary>
        /// Reads 4 bytes in big endian and returns a signed 32-bit integer.
        /// </summary>
        /// <param name="br">
        /// A <see cref="BinaryReader"/> that has a int in big endian to read.
        /// </param>
        /// <returns>A signed 32-bit integer.</returns>
        public static int ReadInt32BE(this BinaryReader br)
        {
            var b = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt32(b, 0);
        }

        /// <summary>
        /// Reads 4 bytes in big endian and returns a signed 32-bit integer.
        /// </summary>
        /// <param name="ba">
        /// A byte array that has a int in big endian to read.
        /// </param>
        /// <param name="offset">The position of the data.</param>
        /// <returns>A signed 32-bit integer.</returns>
        public static int ReadInt32BE(this byte[] ba, int offset = 0)
        {
            var b = new byte[4];
            Array.Copy(ba, offset, b, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToInt32(b, 0);
        }

        /// <summary>
        /// Writes a value to an array in big endian.
        /// </summary>
        /// <param name="ba">The array to write to.</param>
        /// <param name="offset">The starting position to write to.</param>
        /// <param name="value">The signed 32-bit integer to write.</param>
        public static void WriteBE(this byte[] ba, int offset, int value)
        {
            var b = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            Array.Copy(b, 0, ba, offset, b.Length);
        }

        /// <summary>
        /// Reads 4 bytes in big endian and returns an usigned 32-bit integer.
        /// </summary>
        /// <param name="br">
        /// A <see cref="BinaryReader"/> that has a int in big endian to read.
        /// </param>
        /// <returns>An usigned 32-bit integer.</returns>
        public static uint ReadUInt32BE(this BinaryReader br)
        {
            var b = br.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToUInt32(b, 0);
        }

        /// <summary>
        /// Reads 4 bytes in big endian and returns a signed 32-bit integer.
        /// </summary>
        /// <param name="ba">
        /// A byte array that has a int in big endian to read.
        /// </param>
        /// <param name="offset">The position of the data.</param>
        /// <returns>A signed 32-bit integer.</returns>
        public static uint ReadUInt32BE(this byte[] ba, int offset = 0)
        {
            var b = new byte[4];
            Array.Copy(ba, offset, b, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            return BitConverter.ToUInt32(b, 0);
        }

        /// <summary>
        /// Writes a value to an array in big endian.
        /// </summary>
        /// <param name="ba">The array to write to.</param>
        /// <param name="offset">The starting position to write to.</param>
        /// <param name="value">The unsigned 32-bit integer to write.</param>
        public static void WriteBE(this byte[] ba, int offset, uint value)
        {
            var b = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);
            Array.Copy(b, 0, ba, offset, b.Length);
        }

        /// <summary>
        /// Reads 7 bytes and returns a timestamp.
        /// </summary>
        /// <param name="ba">
        /// A byte array that has a DateTime in AS/400 serialization format to
        /// read.
        /// </param>
        /// <param name="offset">The position of the data.</param>
        /// <returns>A timestamp.</returns>
        public static DateTime ReadDateTime(this byte[] ba, int offset = 0)
        {
            var year = ba.ReadUInt16BE(offset);
            var month = ba[2 + offset];
            var day = ba[3 + offset];
            var hour = ba[4 + offset];
            var minute = ba[5 + offset];
            var second = ba[6 + offset];
            var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            return dt;
        }
    }
}

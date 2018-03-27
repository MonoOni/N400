using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Globalization
{
    /// <summary>
    /// Contains utilities for converting to and from EBCDIC text.
    /// </summary>
    public static class EbcdicConverter
    {
        // TODO: support other than NLV 2924? padding functions?

        /// <summary>
        /// Pads a byte string/array to a certain size.
        /// </summary>
        /// <param name="s">The byte array/string to pad</param>
        /// <param name="length">The length of the new buffer.</param>
        /// <param name="padWith">
        /// The item to pad with. By default, a space in EBCDIC.
        /// </param>
        /// <returns>The original array/string, padded.</returns>
        public static byte[] ToPadded(byte[] s, int length, byte padWith = 0x40)
        {
            var b = new byte[length];
            for (var i = 0; length > i; i++)
                b[i] = padWith;
            Array.Copy(s, b, Math.Min(s.Length, length));
            return b;
        }

        /// <summary>
        /// Converts a string from a source encoding to EBCDIC.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="nlv">
        /// The national language version to use.
        /// </param>
        /// <param name="sourceEncoding">
        /// The encoding to convert from. If null, (by default) ASCII is
        /// assumed.
        /// </param>
        /// <returns>The EBCDIC string in a byte array.</returns>
        public static byte[] ToEbcidic(string source, int nlv = 2924, Encoding sourceEncoding = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (nlv != 2924)
                throw new NotImplementedException("Only NLV 2924 is supported.");

            sourceEncoding = sourceEncoding ?? Encoding.ASCII;
            var sourceBytes = sourceEncoding.GetBytes(source);
            return ToEbcidic(sourceBytes, nlv, sourceEncoding);
        }

        /// <summary>
        /// Converts a byte array from a source encoding to EBCDIC.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="nlv">
        /// The national language version to use.
        /// </param>
        /// <param name="sourceEncoding">
        /// The encoding to convert from. If null, (by default) ASCII is
        /// assumed.
        /// </param>
        /// <returns>The EBCDIC string in a byte array.</returns>
        public static byte[] ToEbcidic(byte[] source, int nlv = 2924, Encoding sourceEncoding = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (nlv != 2924)
                throw new NotImplementedException("Only NLV 2924 is supported.");

            var ebcdic = Encoding.GetEncoding("IBM037");
            sourceEncoding = sourceEncoding ?? Encoding.ASCII;

            return Encoding.Convert(sourceEncoding, ebcdic, source);
        }

        /// <summary>
        /// Converts an EBCDIC byte array to a byte array in a new encoding.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="nlv">
        /// The national language version to use.
        /// </param>
        /// <param name="destEncoding">
        /// The encoding to convert to. If null, (by default) ASCII is assumed.
        /// </param>
        /// <returns>The converted text in a byte array.</returns>
        public static byte[] FromEbcidicToArray(byte[] source, int nlv = 2924, Encoding destEncoding = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (nlv != 2924)
                throw new NotImplementedException("Only NLV 2924 is supported.");

            var ebcdic = Encoding.GetEncoding("IBM037");
            destEncoding = destEncoding ?? Encoding.ASCII;

            return Encoding.Convert(ebcdic, destEncoding, source);
        }

        /// <summary>
        /// Converts an EBCDIC byte array to a string in a new encoding.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="nlv">
        /// The national language version to use.
        /// </param>
        /// <param name="destEncoding">
        /// The encoding to convert to. If null, (by default) ASCII is assumed.
        /// </param>
        /// <returns>The converted text in a string.</returns>
        public static string FromEbcidicToString(byte[] source, int nlv = 2924, Encoding destEncoding = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (nlv != 2924)
                throw new NotImplementedException("Only NLV 2924 is supported.");

            destEncoding = destEncoding ?? Encoding.ASCII;
            return destEncoding.GetString(FromEbcidicToArray(source, nlv, destEncoding));
        }
    }
}

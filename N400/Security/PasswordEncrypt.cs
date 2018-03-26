using N400.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace N400.Security
{
    internal static class PasswordEncrypt
    {
        static SHA1 sha1 = SHA1.Create();

        public static byte[] EncryptPassword(string userId, string password, byte[] clientSeed, byte[] serverSeed, int passwordLevel)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (userId == string.Empty)
                throw new ArgumentException("Username cannot be empty.", nameof(userId));
            if (userId.Length > 10)
                throw new ArgumentException("Username too long.", nameof(userId));
            if (password == string.Empty)
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            // check password length per function, instead
            if (clientSeed == null)
                throw new ArgumentNullException(nameof(clientSeed));
            if (serverSeed == null)
                throw new ArgumentNullException(nameof(serverSeed));

            // http://www-01.ibm.com/support/docview.wss?uid=nas8N1016481 is useful?
            if (passwordLevel == 0)
                return EncryptPasswordDES(userId, password, clientSeed, serverSeed);
            else
                return EncryptPasswordSHA1(userId, password, clientSeed, serverSeed);
        }

        static byte[] EncryptPasswordSHA1(string userId, string password, byte[] clientSeed, byte[] serverSeed)
        {
            if (password.Length > 128)
                throw new ArgumentException("Password too long.", nameof(password));

            var sequence = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 1
            };

            // TODO: needs to be updated with EbcidicConverter
            // it needs to be big endian UTF-16 as an intermediate...?
            var userBuf = Encoding.Convert(Encoding.GetEncoding("IBM037"), Encoding.BigEndianUnicode, EbcidicConverter.ToEbcidic(userId));
            var passwordBuf = Encoding.Convert(Encoding.ASCII, Encoding.BigEndianUnicode, Encoding.ASCII.GetBytes(password));
            
            var passwordToken = sha1.ComputeHash(userBuf
                .Concat(passwordBuf)
                .ToArray());
            var finalHash = sha1.ComputeHash(passwordToken
                .Concat(serverSeed)
                .Concat(clientSeed)
                .Concat(userBuf)
                .Concat(sequence)
                .ToArray());

            return finalHash;
        }

        static byte[] EncryptPasswordDES(string userId, string password, byte[] clientSeed, byte[] serverSeed)
        {
            if (password.Length > 10)
                throw new ArgumentException("Password too long.", nameof(password));

            // all values are upper case
            var userEbcidic = EbcidicConverter.ToEbcidic(userId.ToUpperInvariant());
            var passEbcidic = EbcidicConverter.ToEbcidic(password.ToUpperInvariant());
            // make fixed sized 0x40-filled buffers
            var userBuf = EbcidicConverter.ToPadded(userEbcidic, 10);
            var passBuf = EbcidicConverter.ToPadded(passEbcidic, 10);
            return PasswordEncryptDES.EncryptPassword(userBuf, passBuf, clientSeed, serverSeed);
        }
    }
}

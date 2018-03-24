using N400.Globalization;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class SignonInfoResponse : ResponsePacket
    {
        public const int LENGTH = 147;
        const ushort ID = 0xF004;
        const ushort USERID = 0x1104;
        const ushort CURRENT_SIGNON_DATE = 0x1106;
        const ushort LAST_SIGNON_DATE = 0x1107;
        const ushort PASSWORD_EXPIRATION_DATE = 0x1108;
        const ushort SERVER_CCSID = 0x1114;
        const ushort EXPIRATION_WARNING = 0x112C;
        // there are a bunch of fields not in here because I don't know what's
        // actually in them:
        // [2]: 4361 // ** UNKNOWN: value 0, ushort
        // [3]: 4362 // ** UNKNOWN: value 3, ushort
        // [4]: 4366 // ** UNKNOWN: value 243, byte
        // [6]: 4363 // ** UNKNOWN: 8-byte buffer, possibly [u]long? long as a
        //                 password as well
        // [8]: 4394 // ** UNKNOWN: value 1, ushort
        // [9]: 4395 // ** UNKNOWN: EBCIDIC strings, looks like structured data
        //                 that may possibly be a 5250 data stream (strings
        //                 seem to suggest it, anyways)

        public DateTime CurrentSignonDate
        {
            get
            {
                return GetField(CURRENT_SIGNON_DATE).ReadDateTime();
            }
            set
            {
                SetField(value, 24, CURRENT_SIGNON_DATE);
            }
        }

        public DateTime LastSignonDate
        {
            get
            {
                return GetField(LAST_SIGNON_DATE).ReadDateTime();
            }
            set
            {
                SetField(value, 38, LAST_SIGNON_DATE);
            }
        }

        public uint ServerCCSID
        {
            get
            {
                return GetField(SERVER_CCSID).ReadUInt32BE();
            }
            set
            {
                SetField(value, 99, SERVER_CCSID);
            }
        }

        public uint ExpirationWarning
        {
            get
            {
                return GetField(EXPIRATION_WARNING).ReadUInt32BE();
            }
            set
            {
                SetField(value, 75, EXPIRATION_WARNING);
            }
        }

        public string UserID
        {
            get
            {
                var ebcidic = GetField(USERID);
                if (ebcidic == null)
                    return null;
                else
                    return EbcidicConverter.FromEbcidicToString(ebcidic);
            }
            set
            {
                var ebcidic = EbcidicConverter.ToEbcidic(value);
                var userBuf = new byte[10].Select(x => (byte)0x40).ToArray();
                Array.Copy(ebcidic, userBuf, Math.Min(ebcidic.Length, 10));
                SetField(userBuf, 137, USERID);
            }
        }

        public DateTime? PasswordExpirationDate
        {
            get
            {
                var field = GetField(PASSWORD_EXPIRATION_DATE);
                return field?.ReadDateTime();
            }
            set
            {
                if (value.HasValue)
                    SetField(value.Value, 117, PASSWORD_EXPIRATION_DATE);
            }
        }

        public SignonInfoResponse() : base(LENGTH)
        {
            PacketLength = LENGTH;
            RequestResponseID = ID;
            ServiceID = SignonService.SERVICE_ID;
        }

        public SignonInfoResponse(byte[] data) : base(data)
        {
        }
    }
}

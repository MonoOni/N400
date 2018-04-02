using N400.Globalization;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class SignonInfoResponse : SignonReturnCodeResponseBase
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
        // [9]: 4395 // ** UNKNOWN: EBCDIC strings, looks like structured data
        //                 that may possibly be a 5250 data stream (strings
        //                 seem to suggest it, anyways)

        public DateTime CurrentSignonDate => GetField(CURRENT_SIGNON_DATE).ReadDateTime();

        public DateTime LastSignonDate => GetField(LAST_SIGNON_DATE).ReadDateTime();

        public uint ServerCCSID => GetField(SERVER_CCSID).ReadUInt32BE();

        public uint ExpirationWarning => GetField(EXPIRATION_WARNING).ReadUInt32BE();

        public string UserID
        {
            get
            {
                var ebcdic = GetField(USERID);
                if (ebcdic == null)
                    return null;
                else
                    return EbcdicConverter.FromEbcidicToString(ebcdic);
            }
        }

        public DateTime? PasswordExpirationDate
        {
            get
            {
                var field = GetField(PASSWORD_EXPIRATION_DATE);
                return field?.ReadDateTime();
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

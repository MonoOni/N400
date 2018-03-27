using N400.Globalization;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class SignonInfoRequest : Packet
    {
        const int BASE_LENGTH = 37;
        const ushort ID = 0x7004;
        const ushort CLIENT_CCSID = 0x1113;
        const ushort USERID = 0x1104;
        const ushort PASSWORD = 0x1105;
        const ushort RETURN_ERROR_MESSAGES = 0x1128;

        public byte AuthenticationScheme
        {
            get
            {
                return Data[20];
            }
            set
            {
                Data[20] = value;
            }
        }

        public uint ClientCCSID
        {
            get
            {
                return GetField(CLIENT_CCSID).ReadUInt32BE();
            }
            set
            {
                Data.WriteBE(21, 10);
                Data.WriteBE(25, CLIENT_CCSID);
                Data.WriteBE(27, value);
            }
        }

        public byte[] Password
        {
            get
            {
                return GetField(PASSWORD);
            }
            set
            {
                Data.WriteBE(31, value.Length + 6);
                Data.WriteBE(35, PASSWORD);
                Array.Copy(value, 0, Data, 37, value.Length);
            }
        }

        public string UserID
        {
            get
            {
                var asEbcidic = GetField(USERID);
                return EbcdicConverter.FromEbcidicToString(asEbcidic);
            }
            set
            {
                var offset = Password.Length + 37;
                Data.WriteBE(offset, 16);
                Data.WriteBE(offset + 4, USERID);

                var buf = new byte[10].Select(x => (byte)0x40).ToArray();
                var asEbcidic = EbcdicConverter.ToEbcidic(value);
                Array.Copy(asEbcidic, 0, buf, 0, Math.Min(10, asEbcidic.Length));
                Array.Copy(buf, 0, Data, offset + 6, 10);
            }
        }

        public bool ReturnErrorMessages
        {
            get
            {
                return (GetField(RETURN_ERROR_MESSAGES)?[0] ?? 0) == 1; 
            }
            set
            {
                var offset = Password.Length + 37 + 16;
                Data.WriteBE(offset, 7);
                Data.WriteBE(offset + 4, RETURN_ERROR_MESSAGES);
                Data[offset + 6] = value ? (byte)0x1 : (byte)0x0;
            }
        }

        public SignonInfoRequest(string userId, byte[] encryptedPassword, ushort serverLevel)
            : base(37 + encryptedPassword.Length + 16 + (serverLevel > 5 ? 7 : 0))
        {
            PacketLength = Data.Length; // this is variable cuz base() call
            TemplateLength = 1;
            RequestResponseID = ID;
            ServiceID = SignonService.SERVICE_ID;

            AuthenticationScheme = 1;
            Password = encryptedPassword;
            ClientCCSID = 1200;
            UserID = userId;
            if (serverLevel >= 5)
                ReturnErrorMessages = true;
        }
    }
}

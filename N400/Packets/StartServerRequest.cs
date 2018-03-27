using N400.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class StartServerRequest : Packet
    {
        const ushort ID = 0x7002;
        const ushort USERID = 0x1104;
        const ushort PASSWORD = 0x1105;

        public byte ClientAttributes
        {
            get
            {
                return Data[4];
            }
            set
            {
                Data[4] = value;
            }
        }

        public byte AuthType
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

        public byte SendReply
        {
            get
            {
                return Data[21];
            }
            set
            {
                Data[21] = value;
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
                Data.WriteBE(22, value.Length + 6);
                Data.WriteBE(26, PASSWORD);
                Array.Copy(value, 0, Data, 28, value.Length);
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
                var offset = Password.Length + 28;
                Data.WriteBE(offset, 16);
                Data.WriteBE(offset + 4, USERID);

                var buf = new byte[10].Select(x => (byte)0x40).ToArray();
                var asEbcidic = EbcdicConverter.ToEbcidic(value);
                Array.Copy(asEbcidic, 0, buf, 0, Math.Min(10, asEbcidic.Length));
                Array.Copy(buf, 0, Data, offset + 6, 10);
            }
        }

        public StartServerRequest(string userId, byte[] password, ushort serviceId)
            : base(44 + password.Length)
        {
            RequestResponseID = ID;
            ServiceID = ServiceID;
            TemplateLength = 2;

            Password = password;
            UserID = userId;

            ClientAttributes = 2; // for job info
            AuthType = 1; // password
            SendReply = 1;
        }
    }
}

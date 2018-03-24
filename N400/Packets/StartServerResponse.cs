using N400.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class StartServerResponse : ResponsePacket
    {
        const ushort ID = 0xF002;
        const ushort USERID = 0x1104;
        const ushort JOB_NAME = 0x111F;

        public byte[] JobName
        {
            get
            {
                return GetField(JOB_NAME);
            }
            set
            {
                SetField(value, 40, JOB_NAME);
            }
        }

        public string UserID
        {
            get
            {
                var asEbcidic = GetField(USERID);
                return EbcidicConverter.FromEbcidicToString(asEbcidic);
            }
            set
            {
                var offset = 24;
                Data.WriteBE(offset, 16);
                Data.WriteBE(offset + 4, USERID);

                var buf = new byte[10].Select(x => (byte)0x40).ToArray();
                var asEbcidic = EbcidicConverter.ToEbcidic(value);
                Array.Copy(asEbcidic, 0, buf, 0, Math.Min(10, asEbcidic.Length));
                Array.Copy(buf, 0, Data, offset + 6, 10);
            }
        }

        public StartServerResponse(byte[] data) : base(data)
        {
        }

        public StartServerResponse() : base(71)
        {
        }
    }
}

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

        public byte[] JobName => GetField(JOB_NAME);

        public string UserID
        {
            get
            {
                var asEbcidic = GetField(USERID);
                return EbcdicConverter.FromEbcidicToString(asEbcidic);
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

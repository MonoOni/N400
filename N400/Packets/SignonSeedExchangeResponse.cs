using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class SignonSeedExchangeResponse : ResponsePacket
    {
        public const int LENGTH = 94;
        const ushort ID = 0xF003;
        const ushort SERVER_VERSION = 0x1101;
        const ushort SERVER_LEVEL = 0x1102;
        const ushort SERVER_SEED = 0x1103;
        const ushort PASSWORD_LEVEL = 0x1119;
        const ushort JOB_NAME = 0x111F;

        // a version looks like:       0x00050100
        //                                ^ ^ ^ ^
        // [0] unknown, maybe [1] ushort -/ | | |
        // [1] major -----------------------/ | |
        // [2] minor -------------------------/ |
        // [4] unknown -------------------------/
        //
        // the i systems I've seen give 0x00070100 or 0x00070200
        public Version ServerVersion
        {
            get
            {
                var bytes = GetField(SERVER_VERSION);
                var ver = new Version(bytes[1], bytes[2]);
                return ver;
            }
        }

        public ushort ServerLevel => GetField(SERVER_LEVEL).ReadUInt16BE();

        public byte[] ServerSeed => GetField(SERVER_SEED);

        public byte PasswordLevel => GetField(PASSWORD_LEVEL)[0];

        public string JobName
        {
            get
            {
                var a = GetField(JOB_NAME).TakeWhile(x => x != 0).ToArray();
                return Encoding.Default.GetString(a);
            }
        }

        public SignonSeedExchangeResponse() : base(LENGTH)
        {
            PacketLength = LENGTH;
            RequestResponseID = ID;
            ServiceID = SignonService.SERVICE_ID;
        }

        public SignonSeedExchangeResponse(byte[] data) : base(data)
        {
        }
    }
}

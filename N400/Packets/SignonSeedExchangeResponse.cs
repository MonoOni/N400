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

        public uint ServerVersion
        {
            get
            {
                return GetField(SERVER_VERSION).ReadUInt32BE();
            }
            set
            {
                Data.WriteBE(24, 10);
                Data.WriteBE(28, SERVER_VERSION);
                Data.WriteBE(30, value);
            }
        }

        public ushort ServerLevel
        {
            get
            {
                return GetField(SERVER_LEVEL).ReadUInt16BE();
            }
            set
            {
                Data.WriteBE(34, 8);
                Data.WriteBE(38, SERVER_LEVEL);
                Data.WriteBE(40, value);
            }
        }

        public byte[] ServerSeed
        {
            get
            {
                return GetField(SERVER_SEED);
            }
            set
            {
                Data.WriteBE(42, 14);
                Data.WriteBE(46, SERVER_SEED);
                Array.Copy(value, 0, Data, 48, 8);
            }
        }

        public byte PasswordLevel
        {
            get
            {
                return GetField(PASSWORD_LEVEL)[0];
            }
            set
            {
                Data.WriteBE(56, 7);
                Data.WriteBE(60, PASSWORD_LEVEL);
                Data[62] = value;
            }
        }

        public string JobName
        {
            get
            {
                var a = GetField(JOB_NAME).TakeWhile(x => x != 0).ToArray();
                return Encoding.Default.GetString(a);
            }
            set
            {
                Data.WriteBE(63, 0x1F);
                Data.WriteBE(67, JOB_NAME);
                var b = Encoding.Default.GetBytes(value);
                Array.Copy(b, 0, Data, 69, Math.Min(b.Length, 25));
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

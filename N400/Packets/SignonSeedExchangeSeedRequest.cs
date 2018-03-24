using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class SignonSeedExchangeSeedRequest : Packet
    {
        static Random random = new Random();

        const int LENGTH = 52;
        const ushort ID = 0x7003;
        const ushort CLIENT_ID = 0x1101;
        const ushort CLIENT_DATA_STREAM_LEVEL = 0x1102;
        const ushort CLIENT_SEED = 0x1103;

        public uint ClientID
        {
            get
            {
                return GetField(CLIENT_ID).ReadUInt32BE();
            }
            set
            {
                SetField(value, 20, CLIENT_ID);
            }
        }

        public ushort ClientDataStreamLevel
        {
            get
            {
                return GetField(CLIENT_DATA_STREAM_LEVEL).ReadUInt16BE();
            }
            set
            {
                SetField(value, 30, CLIENT_DATA_STREAM_LEVEL);
            }
        }

        public byte[] ClientSeed
        {
            get
            {
                return GetField(CLIENT_SEED);
            }
            set
            {
                SetField(value, 38, CLIENT_SEED);
            }
        }

        public SignonSeedExchangeSeedRequest() : base(LENGTH)
        {
            PacketLength = LENGTH;
            ServiceID = SignonService.SERVICE_ID;
            RequestResponseID = ID;
            TemplateLength = 0;

            ClientID = 1;
            ClientDataStreamLevel = 5;

            var b = new byte[8];
            random.NextBytes(b);
            ClientSeed = b;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class RandomSeedExchangeRequest : Packet
    {
        static Random random = new Random();

        const ushort ID = 0x7001;

        public byte[] Seed
        {
            get
            {
                return Data.Slice(20, 8);
            }
            set
            {
                Array.Copy(value, 0, Data, 20, Math.Min(value.Length, 8));
            }
        }

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

        public RandomSeedExchangeRequest(ushort serviceId) : base(28)
        {
            TemplateLength = 8;
            ServiceID = serviceId;
            RequestResponseID = ID;

            ClientAttributes = 1; // SHA1

            var bytes = new byte[8];
            random.NextBytes(bytes);
            Seed = bytes;
        }
    }
}

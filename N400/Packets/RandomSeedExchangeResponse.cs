using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class RandomSeedExchangeResponse : ResponsePacket
    {
        const ushort ID = 0xF001;

        public byte[] Seed
        {
            get
            {
                return Data.Slice(24, 8);
            }
            set
            {
                Array.Copy(value, 0, Data, 24, Math.Min(value.Length, 8));
            }
        }

        public RandomSeedExchangeResponse(byte[] data) : base(data)
        {
        }

        public RandomSeedExchangeResponse() : base(32)
        {
            TemplateLength = 8;
            RequestResponseID = ID;
        }
    }
}

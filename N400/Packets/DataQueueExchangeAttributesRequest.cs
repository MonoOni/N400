using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueExchangeAttributesRequest : Packet
    {
        public uint ClientVersion
        {
            get
            {
                return Data.ReadUInt32BE(20);
            }
            set
            {
                Data.WriteBE(20, value);
            }
        }

        public DataQueueExchangeAttributesRequest() : base(26)
        {
            TemplateLength = 6;
            ServiceID = DataQueueService.SERVICE_ID;

            ClientVersion = 1;
        }
    }
}

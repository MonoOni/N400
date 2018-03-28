using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueReadResponse : Packet
    {
        public const ushort ID = 0x8003;
        const ushort ENTRY = 0x5001;
        const ushort KEY = 0x5002;

        public byte[] SenderInfo
        {
            get
            {
                return Data.Slice(22, 30);
            }
            set
            {
                Array.Copy(value, 0, Data, 22, Math.Min(value.Length, 30));
            }
        }

        public byte[] Entry
        {
            get
            {
                return GetField(ENTRY);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public byte[] Key
        {
            get
            {
                return GetField(KEY);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DataQueueReadResponse() : base(58)
        {
            TemplateLength = 38;
            ServiceID = DataQueueService.SERVICE_ID;
            RequestResponseID = ID;
        }

        public DataQueueReadResponse(byte[] data) : base(data)
        {
        }
    }
}

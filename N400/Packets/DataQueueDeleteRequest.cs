using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueDeleteRequest : Packet
    {
        const ushort ID = 0x0004;

        public byte[] Name
        {
            get
            {
                return Data.Slice(20, 10);
            }
            set
            {
                Array.Copy(value, 0, Data, 20, Math.Min(value.Length, 10));
            }
        }

        public byte[] Library
        {
            get
            {
                return Data.Slice(30, 10);
            }
            set
            {
                Array.Copy(value, 0, Data, 30, Math.Min(value.Length, 10));
            }
        }

        public DataQueueDeleteRequest(byte[] name, byte[] library) : base(40)
        {
            TemplateLength = 20;
            ServiceID = DataQueueService.SERVICE_ID;
            RequestResponseID = ID;

            Name = name;
            Library = library;
        }
    }
}

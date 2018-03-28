using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal abstract class DataQueueRequestBase : Packet
    {
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

        public DataQueueRequestBase(int length, byte[] name, byte[] library)
            : base(length)
        {
            TemplateLength = 20; // for name and lib, children may extend
            ServiceID = DataQueueService.SERVICE_ID;

            Name = name;
            Library = library;
        }
    }
}

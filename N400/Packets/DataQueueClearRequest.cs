using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueClearRequest : Packet
    {
        const ushort ID = 0x0006;
        const ushort KEY = 0x5002;

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

        public byte[] Key
        {
            get
            {
                if (Data[40] == 0xF1)
                    return GetField(KEY);
                else return null;
            }
            set
            {
                if (value != null)
                {
                    Data[40] = 0xF1;
                    SetField(value, 41, KEY);
                }
                else
                {
                    Data[40] = 0xF0;
                }
            }
        }

        public DataQueueClearRequest(byte[] name, byte[] library, byte[] key)
            : base(key == null ? 41 : 47 + key.Length)
        {
            TemplateLength = 21;
            ServiceID = DataQueueService.SERVICE_ID;
            RequestResponseID = ID;

            Name = name;
            Library = library;

            if (key != null)
            Key = key;
        }
    }
}

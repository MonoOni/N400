using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueWriteRequest : Packet
    {
        public const ushort ID = 0x0005;
        const ushort ENTRY = 0x5001;
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

        public byte[] Entry
        {
            get
            {
                return GetField(ENTRY);
            }
            set
            {
                SetField(value, 48, ENTRY);
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

                    var offset = 48 + (6 + Entry.Length);
                    SetField(value, offset, KEY);
                }
                else
                {
                    Data[40] = 0xF0;
                }
            }
        }

        public DataQueueWriteRequest(byte[] name, byte[] library, byte[] entry, byte[] key)
            : base(key == null ? 48 + entry.Length : 54 + entry.Length + key.Length)
        {
            TemplateLength = 22;
            ServiceID = DataQueueService.SERVICE_ID;
            RequestResponseID = ID;

            Name = name;
            Library = library;

            Entry = entry;
            Data[41] = 0xF1; // we want a reply
            if (key != null)
                Key = key;
        }
    }
}

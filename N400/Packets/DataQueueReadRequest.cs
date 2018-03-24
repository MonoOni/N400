using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class DataQueueReadRequest : Packet
    {
        const ushort ID = 0x0002;
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

        // XXX: What is this?
        public ushort Search
        {
            get
            {
                return Data.ReadUInt16BE(41);
            }
            set
            {
                Data.WriteBE(41, value);
            }
        }

        public int Wait
        {
            get
            {
                return Data.ReadInt32BE(43);
            }
            set
            {
                Data.WriteBE(43, value);
            }
        }

        public bool Peek
        {
            get
            {
                return Data[47] == 0xF1;
            }
            set
            {
                Data[47] = (byte)(value ? 0xF1 : 0xF0);
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
                    SetField(value, 48, KEY);
                }
                else
                {
                    Data[40] = 0xF0;
                }
            }
        }

        public DataQueueReadRequest(byte[] name, byte[] library, int wait, bool peek, byte[] key)
            : base(key == null ? 48 : 54 + key.Length)
        {
            TemplateLength = 28;
            ServiceID = DataQueueService.SERVICE_ID;
            RequestResponseID = ID;

            Name = name;
            Library = library;
            Wait = wait;
            Peek = peek;
            Key = key;
        }
    }
}

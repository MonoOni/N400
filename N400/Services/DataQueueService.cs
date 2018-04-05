using N400.DataQueues;
using N400.Globalization;
using N400.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    internal class DataQueueService : Service
    {
        public const ushort SERVICE_ID = 0xE007;

        public DataQueueService(Server server)
            : base (server, SERVICE_ID, "as-dtaq", null, 8472, 9472)
        {
        }

        protected override bool Initialize()
        {
            var exchangeRequest = new DataQueueExchangeAttributesRequest();
            WritePacket(exchangeRequest);
            // we need to check the length and/or ID to then cast it later
            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            // DataQueueReturnCodeResponse
            else if (res.RequestResponseID == DataQueueReturnCodeResponse.ID)
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                throw new Exception(
                    "An error occured when exchanging attributes with the data queue service: " +
                    $"{rcRes.ReturnCode}");
            }
            // DataQueueExchangeAttributesResponse (basically just Packet)
            else if (res.RequestResponseID == 0x8000)
            {
                // there's nothing in the packet, so this is good
                // so do nothing and fall through
            }
            else
                throw new Exception("Invalid data queue attributes exchange response ID " +
                    $"{res.RequestResponseID}");

            return true;
        }

        public void Create(byte[] name,
            byte[] library,
            uint entryLength,
            bool saveSenderInfo,
            bool fifo,
            DataQueueAuthority authority,
            uint keyLength,
            bool forceStorage,
            byte[] description)
        {
            EnsureInitialized();

            var createReq = new DataQueueCreateRequest(name,
                library,
                entryLength,
                saveSenderInfo,
                fifo,
                authority,
                keyLength,
                forceStorage,
                description);
            WritePacket(createReq);

            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            else if (res.RequestResponseID != DataQueueReturnCodeResponse.ID)
                throw new Exception($"Invalid data queue create response ID {res.RequestResponseID}");
            else
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                if (rcRes.ReturnCode != 0xF000)
                    throw new Exception(
                        "An error occured when creating a queue with the data queue service: " +
                        $"{rcRes.ReturnCode}");
            }
        }

        public void Delete(byte[] name, byte[] library)
        {
            EnsureInitialized();

            var deleteReq = new DataQueueDeleteRequest(name, library);
            WritePacket(deleteReq);

            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            else if (res.RequestResponseID != DataQueueReturnCodeResponse.ID)
                throw new Exception($"Invalid data queue delete response ID {res.RequestResponseID}");
            else
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                if (rcRes.ReturnCode != 0xF000)
                    throw new Exception(
                        "An error occured when deleting a queue on the data queue service: " +
                        $"{rcRes.ReturnCode}");
            }
        }

        public void Clear(byte[] name, byte[] library, byte[] key)
        {
            EnsureInitialized();

            var clearReq = new DataQueueClearRequest(name, library, key);
            WritePacket(clearReq);

            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            else if (res.RequestResponseID != DataQueueReturnCodeResponse.ID)
                throw new Exception($"Invalid data queue clear response ID {res.RequestResponseID}");
            else
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                if (rcRes.ReturnCode != 0xF000)
                    throw new Exception(
                        "An error occured when clearing a queue on the data queue service: " +
                        $"{rcRes.ReturnCode}");
            }
        }

        public void Write(byte[] name, byte[] library, byte[] entry, byte[] key)
        {
            EnsureInitialized();

            var writeReq = new DataQueueWriteRequest(name, library, entry, key);
            WritePacket(writeReq);

            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            else if (res.RequestResponseID != DataQueueReturnCodeResponse.ID)
                throw new Exception($"Invalid data queue write response ID {res.RequestResponseID}");
            else
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                if (rcRes.ReturnCode != 0xF000)
                    throw new Exception(
                        "An error occured when writing to a queue on the data queue service: " +
                        $"{rcRes.ReturnCode}");
            }
        }

        public DataQueueEntry Read(byte[] name, byte[] library, ushort search, int wait, bool peek, byte[] key)
        {
            EnsureInitialized();

            // TODO: wire up search once we know what it does
            var readReq = new DataQueueReadRequest(name, library, wait, peek, key);
            WritePacket(readReq);

            var res = ReadPacket<Packet>();
            if (res.PacketLength < 22)
                throw new Exception("Invalid packet length.");
            else if (res.RequestResponseID == DataQueueReturnCodeResponse.ID)
            {
                var rcRes = new DataQueueReturnCodeResponse(res.Data);
                if (rcRes.ReturnCode != 0xF006)
                    throw new Exception(
                        "An error occured when reading from a queue on the data queue service: " +
                        $"{rcRes.ReturnCode}");
                else
                    return null; // nothing here
            }
            else if (res.RequestResponseID == DataQueueReadResponse.ID)
            {
                var readRes = new DataQueueReadResponse(res.Data);
                var senderInfoString = EbcdicConverter.FromEbcidicToString(readRes.SenderInfo,
                    Server.NLV);
                var entry = new DataQueueEntry(readRes.Entry,
                    senderInfoString,
                    readRes.SenderInfo);
                return entry;
            }
            else
                throw new Exception("Invalid data queue read response ID " +
                    $"{res.RequestResponseID}");
        }
    }
}

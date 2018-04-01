using N400.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    internal class IfsService : Service
    {
        public const ushort SERVICE_ID = 0xE002;

        bool attributesExchanged;
        ushort level;

        public IfsService(Server server) :
            base(server, SERVICE_ID, "as-file", null, 8473, 9473)
        {
            attributesExchanged = false;
            level = 0;
        }

        void EnsureInitialized()
        {
            if (!Connected)
            {
                Connect();
                if (!Connected)
                    throw new Exception("Couldn't connect to the file service.");

                attributesExchanged = false;
            }
            if (!attributesExchanged)
            {
                // ...signon returns uint but we need ushort?

                // this is unicode because ebcdic makes no sense for unix names
                var infoReq = new IfsExchangeAttributesRequest(
                    new ushort[] {
                        1200, // UTF-16
                        13488, // UTF-16 big endian
                        61952, // legacy UTF-16 BE
                    });
                WritePacket(infoReq);

                var infoRes = ReadPacket<IfsExchangeAttributesResponse>();
                if (infoRes.PacketLength < 22)
                    throw new Exception("The packet returned was too small.");
                if (infoRes.ReturnCode != 0)
                    throw new Exception(
                        "An error occured when exchanging attributes with the file service: " +
                        $"{infoRes.ReturnCode}");

                level = infoRes.DataStreamLevel;

                attributesExchanged = true;
            }
        }

        public IEnumerable<string> ListFiles(string path)
        {
            var pathBytes = Encoding.BigEndianUnicode.GetBytes(path);

            throw new NotImplementedException();
        }
    }
}

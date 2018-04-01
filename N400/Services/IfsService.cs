using N400.FileSystem;
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
        const ushort FILE_NOT_FOUND = 2;
        const ushort PATH_NOT_FOUND = 3;
        const ushort ACCESS_DENIED = 13;
        const ushort NO_MORE_FILES = 18;

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

        public IEnumerable<FileAttributes> List(string path)
        {
            EnsureInitialized();

            // append a * if there's a /, it's picky about trailing slashes
            if (path.EndsWith("/"))
                path += "*";

            // get the base dir
            var lastSlash = path.LastIndexOf("/");
            var basePath = path.Substring(0, lastSlash);

            var pathBytes = Encoding.BigEndianUnicode.GetBytes(path);

            var listReq = new IfsListAttributesRequest(pathBytes);
            WritePacket(listReq);

            ushort chain = 1;
            while (chain != 0)
            {
                var listResBoxed = ReadPacket<Packet>();

                if (listResBoxed.RequestResponseID == IfsReturnCodeResponse.ID)
                {
                    var infoRes = new IfsReturnCodeResponse(listResBoxed.Data);
                    chain = infoRes.Chain;
                    switch (infoRes.ReturnCode)
                    {
                        case NO_MORE_FILES:
                            goto end;
                        default:
                            throw new Exception($"The file service returned an error: {infoRes.ReturnCode}");
                    }
                }
                else if (listResBoxed.RequestResponseID == IfsListAttributeResponse.ID)
                {
                    var infoRes = new IfsListAttributeResponse(listResBoxed.Data);
                    chain = infoRes.Chain;
                    var nameString = Encoding.BigEndianUnicode.GetString(infoRes.FileName);
                    var fullPath = string.Format("{0}/{1}", basePath, nameString);

                    var attributes = new FileAttributes(nameString,
                        fullPath,
                        infoRes.ObjectType == 2,
                        infoRes.Symlink,
                        infoRes.FileSize,
                        infoRes.CreationDate,
                        infoRes.ModificationDate,
                        infoRes.AccessDate,
                        infoRes.FileCCSID,
                        infoRes.Version);
                    yield return attributes;
                }
                else
                    throw new Exception($"The file service returned an unknown packet ID: {listResBoxed.RequestResponseID}");
            }
            end:
            ;
        }
    }
}

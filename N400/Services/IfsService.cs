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

        public IfsService(Server server) :
            base(server, SERVICE_ID, "as-file", null, 8473, 9473)
        {
            attributesExchanged = false;
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

                if (infoRes.DataStreamLevel < 16)
                    throw new Exception("The data stream level doesn't support the necessary features.");

                attributesExchanged = true;
            }
        }

        static byte[] BigEndianBytes(string s) => Encoding.BigEndianUnicode.GetBytes(s);

        public List<FileAttributes> List(string path, PatternMatchingMode globMode)
        {
            EnsureInitialized();

            // get the base dir
            var lastSlash = path.LastIndexOf("/");
            var basePath = path.Substring(0, lastSlash);

            var pathBytes = BigEndianBytes(path);

            var listReq = new IfsListAttributesRequest(pathBytes, globMode);
            WritePacket(listReq);

            var list = new List<FileAttributes>();

            ushort chain = 1;
            while (chain != 0)
            {
                var boxed = ReadPacket<Packet>();

                if (boxed.RequestResponseID == IfsReturnCodeResponse.ID)
                {
                    var listRes = new IfsReturnCodeResponse(boxed.Data);
                    chain = listRes.Chain;
                    switch (listRes.ReturnCode)
                    {
                        case NO_MORE_FILES:
                            goto end;
                        default:
                            throw new Exception($"The file service returned an error: {listRes.ReturnCode}");
                    }
                }
                else if (boxed.RequestResponseID == IfsListAttributeResponse.ID)
                {
                    try
                    {
                        var listRes = new IfsListAttributeResponse(boxed.Data);
                        chain = listRes.Chain;
                        var nameString = Encoding.BigEndianUnicode.GetString(listRes.FileName);
                        var fullPath = string.Format("{0}/{1}", basePath, nameString);

                        var attributes = new FileAttributes(nameString,
                            fullPath,
                            listRes.ObjectType,
                            listRes.Symlink,
                            listRes.FileSize,
                            listRes.CreationDate,
                            listRes.ModificationDate,
                            listRes.AccessDate,
                            listRes.FileCCSID,
                            listRes.Version);
                        list.Add(attributes);
                    }
                    catch (Exception)
                    {
                        // there may be packets left, we can't risk choking on
                        // a parsing error and leaving them there
                        continue;
                    }
                }
                else
                    throw new Exception($"The file service returned an unknown packet ID: {boxed.RequestResponseID}");
            }
            end:
            return list;
        }

        public void DeleteFile(string fileName)
        {
            EnsureInitialized();

            var fileNameBytes = BigEndianBytes(fileName);
            var deleteReq = new IfsDeleteFileRequest(fileNameBytes);
            WritePacket(deleteReq);

            var deleteRes = ReadPacket<IfsReturnCodeResponse>();
            if (deleteRes.ReturnCode != 0)
                throw new Exception($"The file service returned an error: {deleteRes.ReturnCode}");
        }

        public AS400FileStream Open(string fileName,
            OpenMode openMode,
            ShareMode shareMode,
            bool create,
            ushort ccsid)
        {
            EnsureInitialized();

            var fileNameBytes = BigEndianBytes(fileName);
            var openReq = new IfsOpenFileRequest(fileNameBytes,
                openMode,
                shareMode,
                create,
                ccsid);
            WritePacket(openReq);

            var boxed = ReadPacket<Packet>();

            if (boxed.RequestResponseID == IfsReturnCodeResponse.ID)
            {
                var openRes = new IfsReturnCodeResponse(boxed.Data);
                throw new Exception($"The file service returned an error: {openRes.ReturnCode}");
            }
            else if (boxed.RequestResponseID == IfsOpenFileResponse.ID)
            {
                var openRes = new IfsOpenFileResponse(boxed.Data);
                var attribs = new FileAttributes(fileName,
                    fileName,
                    ObjectType.File, // this isn't /exactly/ it, but default to it
                    false,
                    openRes.FileSize,
                    openRes.CreationDate,
                    openRes.ModificationDate,
                    openRes.AccessDate,
                    openRes.FileCCSID,
                    openRes.Version);

                var stream = new AS400FileStream(openRes.Handle, openMode, attribs, this);
                return stream;
            }
            else
                throw new Exception($"The file service returned an unknown packet ID: {boxed.RequestResponseID}");
        }

        public ushort Close(uint handle)
        {
            EnsureInitialized();

            var closeReq = new IfsCloseFileRequest(handle);
            WritePacket(closeReq);

            var closeRes = ReadPacket<IfsReturnCodeResponse>();
            return closeRes.ReturnCode;
        }

        // HACK: i hate that .NET uses signed values for Stream but the AS/400
        // APIs seem to assume unsigned values; so we'll risk it and use
        // signed values for dealing with read and write
        // TODO: Packet chaining situations for read res/write req
        public byte[] Read(uint handle, long offset, int length)
        {
            EnsureInitialized();

            var readReq = new IfsReadRequest(handle, offset, length);
            WritePacket(readReq);
            var boxed = ReadPacket<Packet>();

            if (boxed.RequestResponseID == IfsReturnCodeResponse.ID)
            {
                var openRes = new IfsReturnCodeResponse(boxed.Data);
                throw new Exception($"The file service returned an error: {openRes.ReturnCode}");
            }
            else if (boxed.RequestResponseID == IfsReadResponse.ID)
            {
                var openRes = new IfsReadResponse(boxed.Data);
                return openRes.FieldData;
            }
            else
                throw new Exception($"The file service returned an unknown packet ID: {boxed.RequestResponseID}");
        }

        public int Write(uint handle, byte[] data, long offset, bool sync, ushort ccsid)
        {
            EnsureInitialized();

            var writeReq = new IfsWriteRequest(handle, data, offset, sync, ccsid);
            WritePacket(writeReq);
            var boxed = ReadPacket<Packet>();

            if (boxed.RequestResponseID == IfsReturnCodeResponse.ID)
            {
                var openRes = new IfsReturnCodeResponse(boxed.Data);
                throw new Exception($"The file service returned an error: {openRes.ReturnCode}");
            }
            else if (boxed.RequestResponseID == IfsWriteResponse.ID)
            {
                var openRes = new IfsWriteResponse(boxed.Data);
                return openRes.BytesNotWritten;
            }
            else
                throw new Exception($"The file service returned an unknown packet ID: {boxed.RequestResponseID}");
        }

        public void Commit(uint handle)
        {
            var commitReq = new IfsCommitRequest(handle);
            WritePacket(commitReq);

            var commitRes = ReadPacket<IfsReturnCodeResponse>();
            if (commitRes.ReturnCode != 0)
                throw new Exception($"The file service returned an error: {commitRes.ReturnCode}");
        }
    }
}

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

        public IfsService(Server server) :
            base(server, SERVICE_ID, "as-file", null, 8473, 9473)
        {
        }

        protected override bool Initialize()
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

            return true;
        }

        // checks validity of input, and if non-success, throw
        static void CheckForError(IfsReturnCodeResponse rcr,
            Packet request = null,
            string path = null)
        {
            if (rcr == null)
                throw new ArgumentNullException(nameof(rcr));
            // in case the service gets *really* confused and returns bogus
            else if (rcr.RequestResponseID != IfsReturnCodeResponse.ID)
                throw new ArgumentException("The packet given wasn't of the right type.", nameof(rcr));

            switch (rcr.ReturnCode)
            {
                case FILE_NOT_FOUND:
                    throw new System.IO.FileNotFoundException("The remote service couldn't find the path.",
                        path);
                case PATH_NOT_FOUND:
                    throw new System.IO.DirectoryNotFoundException("The remote service couldn't find the path.");
                case ACCESS_DENIED:
                    throw new UnauthorizedAccessException("Access was denied to the item.");
                default:
                    throw new Exception($"The file service returned an unknown error: {rcr.ReturnCode}");
                case 0:
                    return;
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
                    // special case because we need to control flow
                    if (listRes.ReturnCode == NO_MORE_FILES)
                        break;
                    else
                        CheckForError(listRes, listReq, path);
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
            return list;
        }

        public void DeleteFile(string fileName)
        {
            EnsureInitialized();

            var fileNameBytes = BigEndianBytes(fileName);
            var deleteReq = new IfsDeleteFileRequest(fileNameBytes);
            WritePacket(deleteReq);

            var deleteRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(deleteRes, deleteReq, fileName);
        }

        public void DeleteDirectory(string pathName)
        {
            EnsureInitialized();

            var pathBytes = BigEndianBytes(pathName);
            var deleteReq = new IfsDeleteDirectoryRequest(pathBytes);
            WritePacket(deleteReq);

            var deleteRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(deleteRes, deleteReq, pathName);
        }

        public void CreateDirectory(string pathName)
        {
            EnsureInitialized();

            var pathBytes = BigEndianBytes(pathName);
            var createReq = new IfsCreateDirectoryRequest(pathBytes);
            WritePacket(createReq);

            var createRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(createRes, createReq, pathName);
        }

        public void Copy(string source,
            string target,
            CopyReplace replace,
            CopyDepth depth)
        {
            EnsureInitialized();

            var sourcePathBytes = BigEndianBytes(source);
            var targetPathBytes = BigEndianBytes(target);
            var copyReq = new IfsCopyRequest(sourcePathBytes,
                targetPathBytes,
                replace,
                depth);
            WritePacket(copyReq);

            var copyRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(copyRes, copyReq, source);
        }

        public void Rename(string source,
            string target,
            bool replace)
        {
            EnsureInitialized();

            var sourcePathBytes = BigEndianBytes(source);
            var targetPathBytes = BigEndianBytes(target);
            var renameReq = new IfsRenameRequest(sourcePathBytes,
                targetPathBytes,
                replace);
            WritePacket(renameReq);

            var renameRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(renameRes, renameReq, source);
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
                CheckForError(openRes, openReq, fileName);
                return null;
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

        public void Close(uint handle)
        {
            EnsureInitialized();

            var closeReq = new IfsCloseFileRequest(handle);
            WritePacket(closeReq);

            var closeRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(closeRes, closeRes, null);
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
                var readRes = new IfsReturnCodeResponse(boxed.Data);
                CheckForError(readRes, readReq, null);
                return null;
            }
            else if (boxed.RequestResponseID == IfsReadResponse.ID)
            {
                var readRes = new IfsReadResponse(boxed.Data);
                return readRes.FieldData;
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
                var writeRes = new IfsReturnCodeResponse(boxed.Data);
                CheckForError(writeRes, writeRes, null);
                return -1;
            }
            else if (boxed.RequestResponseID == IfsWriteResponse.ID)
            {
                var writeRes = new IfsWriteResponse(boxed.Data);
                return writeRes.BytesNotWritten;
            }
            else
                throw new Exception($"The file service returned an unknown packet ID: {boxed.RequestResponseID}");
        }

        public void Commit(uint handle)
        {
            EnsureInitialized();

            var commitReq = new IfsCommitRequest(handle);
            WritePacket(commitReq);

            var commitRes = ReadPacket<IfsReturnCodeResponse>();
            CheckForError(commitRes, commitReq, null);
        }
    }
}

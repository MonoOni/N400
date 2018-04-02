using N400.FileSystem;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsOpenFileRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x0002;

        public ushort FileNameCCSID
        {
            get
            {
                return Data.ReadUInt16BE(22);
            }
            set
            {
                Data.WriteBE(22, value);
            }
        }

        public uint WorkingDirectoryHandle
        {
            get
            {
                return Data.ReadUInt32BE(24);
            }
            set
            {
                Data.WriteBE(24, value);
            }
        }

        public ushort FileCCSID
        {
            get
            {
                return Data.ReadUInt16BE(28);
            }
            set
            {
                Data.WriteBE(28, value);
            }
        }

        public OpenMode OpenMode
        {
            get
            {
                return (OpenMode)Data.ReadUInt16BE(30);
            }
            set
            {
                Data.WriteBE(30, (ushort)value);
            }
        }

        public ShareMode ShareMode
        {
            get
            {
                return (ShareMode)Data.ReadUInt16BE(32);
            }
            set
            {
                Data.WriteBE(32, (ushort)value);
            }
        }

        // 0 none 1 use client 2 use server
        public ushort DataConversion
        {
            get
            {
                return Data.ReadUInt16BE(34);
            }
            set
            {
                Data.WriteBE(34, value);
            }
        }

        // TODO: expose this
        // 16 failIfNoExist+replaceIfExist
        // 8  failIfNoExist+openIfExist
        // 4  createOpenIfNoExist+failIfExist
        // 2  createOpenIfNoExist+replaceIfExist
        // 1=createOpenIfNoExist+openIfExist
        public ushort Duplicate
        {
            get
            {
                return Data.ReadUInt16BE(36);
            }
            set
            {
                Data.WriteBE(36, value);
            }
        }

        // uint create at 38, dont bother when we have large

        public uint FixedAttributes
        {
            get
            {
                return Data.ReadUInt32BE(42);
            }
            set
            {
                Data.WriteBE(42, value);
            }
        }

        public ushort AttributeListLevel
        {
            get
            {
                return Data.ReadUInt16BE(46);
            }
            set
            {
                Data.WriteBE(46, value);
            }
        }

        public uint PreReadOffset
        {
            get
            {
                return Data.ReadUInt32BE(48);
            }
            set
            {
                Data.WriteBE(48, value);
            }
        }

        public uint PreReadLength
        {
            get
            {
                return Data.ReadUInt32BE(52);
            }
            set
            {
                Data.WriteBE(52, value);
            }
        }

        public ulong CreateSize
        {
            get
            {
                return Data.ReadUInt32BE(56);
            }
            set
            {
                Data.WriteBE(56, value);
            }
        }

        public byte[] FileName
        {
            get
            {
                return GetField(0x0002);
            }
            set
            {
                SetField(value, 64, 0x0002);
            }
        }

        public IfsOpenFileRequest(byte[] fileName,
            OpenMode openMode,
            ShareMode shareMode,
            bool create,
            ushort ccsid)
            : base(26 + 44 + fileName.Length)
        {
            ServiceID = IfsService.SERVICE_ID;
            RequestResponseID = ID;
            TemplateLength = 44;

            FileNameCCSID = 1200;
            WorkingDirectoryHandle = 1;
            FileCCSID = ccsid;
            OpenMode = openMode;
            ShareMode = ShareMode;
            DataConversion = 0;
            Duplicate = (ushort)(create ? 1 : 8);
            FixedAttributes = 0;
            AttributeListLevel = 1;
            PreReadOffset = 0;
            PreReadLength = 0;
            CreateSize = 0;
            FileName = fileName;
        }
    }
}

﻿using N400.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Packets
{
    internal class IfsCopyRequest : IfsChainedPacketBase
    {
        const ushort ID = 0x0001;

        public ushort SourceFileNameCCSID
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

        public ushort TargetFileNameCCSID
        {
            get
            {
                return Data.ReadUInt16BE(24);
            }
            set
            {
                Data.WriteBE(24, value);
            }
        }

        public uint SourceWorkingDirectoryHandle
        {
            get
            {
                return Data.ReadUInt32BE(26);
            }
            set
            {
                Data.WriteBE(26, value);
            }
        }

        public uint TargetWorkingDirectoryHandle
        {
            get
            {
                return Data.ReadUInt32BE(30);
            }
            set
            {
                Data.WriteBE(30, value);
            }
        }

        public ushort Flags
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

        public byte[] SourceFileName
        {
            get
            {
                return GetField(0x0003);
            }
            protected set
            {
                SetField(value, 36, 0x0003);
            }
        }

        public byte[] TargetFileName
        {
            get
            {
                return GetField(0x0004);
            }
            protected set
            {
                SetField(value, 36 + 6 + GetField(0x0003)?.Length ?? 0, 0x0004);
            }
        }

        public IfsCopyRequest(byte[] sourcePath,
            byte[] targetPath,
            CopyReplace replace,
            CopyDepth deep)
            : base(48 + sourcePath.Length + targetPath.Length)
        {
            RequestResponseID = ID;
            TemplateLength = 16;

            SourceFileNameCCSID = 1200;
            TargetFileNameCCSID = 1200;
            SourceWorkingDirectoryHandle = 1;
            TargetWorkingDirectoryHandle = 1;
            Flags = (ushort)((ushort)deep + (ushort)replace);
            SourceFileName = sourcePath;
            TargetFileName = targetPath;
        }
    }
}

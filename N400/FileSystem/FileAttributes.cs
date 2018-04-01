using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents the attributes of a file system item.
    /// </summary>
    public class FileAttributes
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        public string FileName { get; protected set; }
        /// <summary>
        /// The full path to the item.
        /// </summary>
        public string FilePath { get; protected set; }
        /// <summary>
        /// If the item is a directory.
        /// </summary>
        public bool Directory { get; protected set; }
        /// <summary>
        /// If the item is a symbolic link.
        /// </summary>
        public bool Symlink { get; protected set; }
        /// <summary>
        /// The size of the item.
        /// </summary>
        public ulong FileSize { get; protected set; }
        /// <summary>
        /// When the item was created.
        /// </summary>
        public DateTime CreationDate { get; protected set; }
        /// <summary>
        /// When the item was last modified.
        /// </summary>
        public DateTime ModificationDate { get; protected set; }
        /// <summary>
        /// When the item was last accessed.
        /// </summary>
        public DateTime AccessDate { get; protected set; }
        /// <summary>
        /// The "coded character set identifier" of the item.
        /// </summary>
        public ushort DataCCSID { get; protected set; }
        /// <summary>
        /// The item's version.
        /// </summary>
        public uint Version { get; protected set; }

        internal FileAttributes(string name,
            string path,
            bool dir,
            bool link,
            ulong size,
            DateTime creation,
            DateTime modification,
            DateTime access,
            ushort ccsid,
            uint version)
        {
            FileName = name;
            FilePath = path;
            Directory = dir;
            Symlink = link;
            FileSize = size;
            CreationDate = creation;
            ModificationDate = modification;
            AccessDate = access;
            DataCCSID = ccsid;
            Version = version;
        }

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns>
        /// A string representation of the item, in this case, the full path.
        /// </returns>
        public override string ToString() => FilePath;
    }
}

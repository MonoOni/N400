using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents the type of a file system item.
    /// </summary>
    public enum ObjectType : ushort
    {
        /// <summary>
        /// A normal "bag of bytes" Unix file.
        /// </summary>
        File = 1,
        /// <summary>
        /// A directory in which items are held.
        /// </summary>
        Directory,
        /// <summary>
        /// A pointer to another item.
        /// </summary>
        SymbolicLink,
        /// <summary>
        /// A special item that represents an OS/400-specific feature.
        /// </summary>
        AS400Object,
        /// <summary>
        /// A "first in, first out" special file.
        /// </summary>
        FifoDevice,
        /// <summary>
        /// A special file that represents character-addressable data.
        /// </summary>
        CharacterDevice,
        /// <summary>
        /// A special file that represents block-addressable data.
        /// </summary>
        BlockDevice,
        /// <summary>
        /// A Unix socket.
        /// </summary>
        Socket
    }
}

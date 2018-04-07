using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents how "deep" a copy operation is for directories.
    /// </summary>
    public enum CopyDepth : ushort
    {
        /// <summary>
        /// Copies only the directory.
        /// </summary>
        SingleItem,
        /// <summary>
        /// Copies the directory and just its immediate children.
        /// </summary>
        Shalow = 4,
        /// <summary>
        /// Copies the directory and its immediate children recursively.
        /// </summary>
        Deep = 8
    }
}

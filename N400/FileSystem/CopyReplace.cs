using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Specifies the replacement behaviour when copying files.
    /// </summary>
    public enum CopyReplace : ushort
    {
        /// <summary>
        /// Don't replace the file.
        /// </summary>
        NoOverwrite,
        /// <summary>
        /// Replaces the target with the source, if existent.
        /// </summary>
        Replace,
        /// <summary>
        /// Appends data from the source to the target.
        /// </summary>
        Append = 3
    }
}

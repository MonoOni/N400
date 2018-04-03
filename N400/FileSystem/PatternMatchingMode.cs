using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents modes for pattern matching in globs.
    /// </summary>
    public enum PatternMatchingMode : ushort
    {
        /// <summary>
        /// Uses POSIX semantics. Returns all items that match the pattern,
        /// and don't begin with a period. If the pattern starts with a period,
        /// then return only those that begin with a period will be returned.
        /// </summary>
        Posix,
        /// <summary>
        /// Like <see cref="Posix"/>, but all matches will be returned.
        /// </summary>
        PosixAll,
        /// <summary>
        /// Use "DOS" (or OS/2) semantics for globbing. Returns all items that
        /// match the pattern.
        /// </summary>
        PC
    }
}

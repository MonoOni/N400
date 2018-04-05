using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents access to the integrated file system on a server.
    /// </summary>
    public class FileSystem
    {
        IfsService service;
        
        /// <summary>
        /// The server to access the integrated file system on.
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        /// Creates a new file system object.
        /// </summary>
        /// <param name="server">
        /// The server to access the integrated file system on.
        /// </param>
        public FileSystem(Server server)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            
            Server = server;

            service = new IfsService(server);
        }

        /// <summary>
        /// Gets file attributes for a path and/or glob.
        /// </summary>
        /// <param name="path">The path specification to list.</param>
        /// <param name="globMode">The method to match patterns by.</param>
        /// <returns>An iterator of file attributes.</returns>
        public List<FileAttributes> List(string path,
            PatternMatchingMode globMode = PatternMatchingMode.PosixAll)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            // append a * if there's a /, it's picky about trailing slashes
            if (path.EndsWith("/"))
                path += "*";

            return service.List(path, globMode);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        public void DeleteFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            service.DeleteFile(path);
        }

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public void DeleteDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            service.DeleteDirectory(path);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        public void CreateDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            service.CreateDirectory(path);
        }

        /// <summary>
        /// Opens a file handle on the server.
        /// </summary>
        /// <param name="path">The path of the item to open.</param>
        /// <param name="openMode">How the item should be opened.</param>
        /// <param name="shareMode">The sharing mode of the item.</param>
        /// <param name="create">
        /// If the item should be created if it doesn't exist.
        /// </param>
        /// <param name="ccsid">
        /// The CCSID of the file. By default, the CCSID for UTF-8.
        /// </param>
        /// <returns>A stream with the file handle.</returns>
        public AS400FileStream Open(string path,
            OpenMode openMode = OpenMode.ReadWrite,
            ShareMode shareMode = ShareMode.All,
            bool create = false,
            ushort ccsid = 1208) // UTF-8
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return service.Open(path, openMode, shareMode, create, ccsid);
        }
    }
}

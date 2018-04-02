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
        /// <returns>An iterator of file attributes.</returns>
        public List<FileAttributes> List(string path)
        {
            return service.List(path);
        }
    }
}

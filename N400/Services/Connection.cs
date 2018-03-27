using N400.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    /// <summary>
    /// Represents a connection for a <see cref="Services.Service"/>.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// The socket for this service.
        /// </summary>
        public Stream Socket { get; private set; }
        /// <summary>
        /// The service this connection is associated with.
        /// </summary>
        public Service Service { get; private set; }
        /// <summary>
        /// The ID used to identify the conversation stream for the connection.
        /// </summary>
        public uint CorrelationId { get; private set; }
        /// <summary>
        /// An EBCDIC string containing the job name for the connection.
        /// </summary>
        public byte[] Job { get; private set; }

        /// <summary>
        /// Creates a connection object.
        /// </summary>
        /// <param name="socket">
        /// The socket for this service.
        /// </param>
        /// <param name="service">
        /// The service this connection is associated with.
        /// </param>
        /// <param name="correlationId">
        /// The ID used to identify the conversation stream for the connection.
        /// </param>
        /// <param name="job">
        /// An EBCDIC string containing the job name for the connection.
        /// </param>
        public Connection(Stream socket, Service service, uint correlationId, byte[] job)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            Socket = socket;
            Service = service;
            CorrelationId = correlationId;
            Job = job;
        }
    }
}

using N400.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    /// <summary>
    /// Represents a connection to a service on a system.
    /// </summary>
    public abstract class Service
    {
        static Random random = new Random();

        /// <summary>
        /// Information, including the socket, associated with the connection
        /// to the server.
        /// </summary>
        public Connection Connection { get; private set; }
        /// <summary>
        /// The server associated to this service.
        /// </summary>
        public Server Server { get; private set; }
        /// <summary>
        /// If the server socket is connected and non-null.
        /// </summary>
        public bool Connected { get; private set; }
        /// <summary>
        /// The ID used when making connections to the server.
        /// </summary>
        public uint ConnectionID { get; private set; }

        /// <summary>
        /// The ID of the service.
        /// </summary>
        public ushort ID { get; private set; }
        /// <summary>
        /// The name of the service to use for port mappings.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The name of the service to use for port mappings when connecting
        /// over TLS.
        /// </summary>
        public string SecureName { get; private set; }
        /// <summary>
        /// The port number of the service.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// The port number of the service when connecting over TLS.
        /// </summary>
        public int SecurePort { get; private set; }

        /// <summary>
        /// Creates a new service.
        /// </summary>
        /// <param name="server">The server associated to this service.</param>
        /// <param name="id">The service's numeric ID.</param>
        /// <param name="name">
        /// The name of the service to use for port mappings.
        /// </param>
        /// <param name="tlsName">
        /// The name of the service to use for port mappings when connecting
        /// over TLS. If this is null, it will be <see cref="Name"/> with "-s"
        /// appended to it.
        /// </param>
        /// <param name="port">The port number of the service.</param>
        /// <param name="tlsPort">
        /// The port number of the service when connecting over TLS.
        /// </param>
        public Service(Server server,
            ushort id,
            string name,
            string tlsName,
            int port,
            int tlsPort)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Server = server;

            ConnectionID = Convert.ToUInt32(random.Next(0, int.MaxValue));

            ID = id;
            Name = name;
            SecureName = tlsName ?? name + "-s";
            Port = port;
            SecurePort = tlsPort;
        }

        /// <summary>
        /// The service's destructor, which will close the held socket, if any.
        /// </summary>
        ~Service()
        {
            Disconnect();
        }
        
        // TODO: is this a reasonable API? should we return a one-shot Socket
        // instead of holding one in the class? or in the server?

        /// <summary>
        /// Establishes a connection to the server, and creates the socket.
        /// </summary>
        protected internal void Connect()
        {
            try
            {
                Connection = Server.CreateConnection(this);
                Connected = Connection != null;
            }
            //catch (Exception e)
            //{
            //    Connected = false;
            //    throw e;
            //}
            finally
            {

            }
        }

        /// <summary>
        /// Closes the connection to the server.
        /// </summary>
        protected internal void Disconnect()
        {
            if (Connected)
            {
                Connection.Socket.Dispose();
                Connection = null;
                Connected = false;
            }
        }

        /// <summary>
        /// Reads a packet from the socket and returns it.
        /// </summary>
        /// <typeparam name="T">
        /// The type of packet to receive and parse.
        /// </typeparam>
        /// <returns>The packet in the socket.</returns>
        /// <remarks>
        /// Note that in error cases, the packet may be smaller than expected,
        /// and as such, are not a full packet. These generally have return
        /// codes, so check the return code before accessing any members not
        /// found in child classes.
        /// </remarks>
        protected internal T ReadPacket<T>() where T : Packet, new()
        {
            if (!Connected)
                throw new Exception("Can't read packets when not connected.");

            return Packet.ReadPacket<T>(Connection.Socket);
        }

        /// <summary>
        /// Writes a packet to the socket.
        /// </summary>
        /// <param name="p">The packet to write</param>
        protected internal void WritePacket(Packet p)
        {
            if (!Connected)
                throw new Exception("Can't write packets when not connected.");

            p.CorrelationID = ConnectionID; // XXX

            Packet.WritePacket(Connection.Socket, p);
        }
    }
}

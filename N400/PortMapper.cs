using N400.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace N400
{
    /// <summary>
    /// Represents access to the port mapper service.
    /// </summary>
    /// <remarks>
    /// The port mapper service as the name implies, maps ports to string IDs.
    /// These IDs allow the server to place services at any port, instead of
    /// assuming the service's location on a hardcoded port.
    /// </remarks>
    internal class PortMapper
    {
        const byte SUCCESS = 0x2B;

        /// <summary>
        /// The hostname of the port mapper service.
        /// </summary>
        public string Hostname { get; private set; }
        /// <summary>
        /// The port of the port mapper service.
        /// </summary>
        /// <remarks>
        /// Default is 449.
        /// </remarks>
        public int Port { get; private set; }

        Dictionary<string, int> cache;

        /// <summary>
        /// Creates a new port mapper object.
        /// </summary>
        /// <param name="hostname">
        /// The hostname of the port mapper service.
        /// </param>
        /// <param name="port">
        /// The port of the port mapper service.
        /// </param>
        public PortMapper(string hostname, int port = 449)
        {
            if (hostname == null)
                throw new ArgumentNullException(nameof(hostname));

            cache = new Dictionary<string, int>();

            Hostname = hostname;
            Port = port;
        }

        /// <summary>
        /// Gets the port for a service.
        /// </summary>
        /// <param name="service">The service to get the port for</param>
        /// <param name="tls">
        /// If the TLS version of the port should be used.
        /// </param>
        /// <param name="mode">
        /// Defines the behaviour used when fetching the mode.
        /// </param>
        /// <returns>The port number for the service.</returns>
        public int GetPort(Service service, bool tls, PortMapperMode mode)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (mode == PortMapperMode.AlwaysAssumed)
                return tls ? service.SecurePort : service.Port;

            try
            {
                return GetPort(tls ? service.SecureName : service.Name);
            }
            catch (Exception e)
            {
                if (mode == PortMapperMode.FallbackToAssumed)
                    return tls ? service.SecurePort : service.Port;
                else throw e;
            }
        }

        /// <summary>
        /// Gets the port for a service by its string ID.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <returns>The port number for the service.</returns>
        public int GetPort(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // check cache first, network operations are expensive
            if (cache.ContainsKey(name))
                return cache[name];

            // not cached, load from port mapper
            var tc = new TcpClient();
            tc.Connect(Hostname, Port);
            if (!tc.Connected)
                throw new Exception("Couldn't connect to the port mapper service.");

            using (var s = tc.GetStream())
            {
                using (var sw = new StreamWriter(s))
                {
                    sw.Write(name);
                    sw.Flush();
                    using (var sr = new BinaryReader(s))
                    {
                        var statusByte = sr.ReadByte();
                        if (statusByte != SUCCESS)
                            throw new Exception($"The port mapper service returned an error: {statusByte}");

                        int port = sr.ReadInt32BE();
                        cache.Add(name, port);
                        return port;
                    }
                }
            }
        }
    }
}

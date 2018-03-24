using N400.Globalization;
using N400.Packets;
using N400.Security;
using N400.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace N400
{
    /// <summary>
    /// Represents an AS/400 server.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The hostname of the server.
        /// </summary>
        public string Hostname { get; private set; }
        /// <summary>
        /// The username of the account used on the server.
        /// </summary>
        public string User { get; private set; }
        /// <summary>
        /// The password of the account used on the server.
        /// </summary>
        public string Password { get; private set; }
        /// <summary>
        /// If TLS should be used when connecting to services.
        /// </summary>
        public bool TLS { get; private set; }
        /// <summary>
        /// The port that the port mapper resides on.
        /// </summary>
        public int PortMapperPort { get; private set; }
        /// <summary>
        /// If the default assumed ports for a service are to be used if the
        /// port mapper is unavailable.
        /// </summary>
        public PortMapperMode PortMapperMode { get; private set; }

        /// <summary>
        /// If the this object has been authenticated to the server.
        /// </summary>
        public bool SignedOn { get; private set; }

        /// <summary>
        /// The "national language version" to use.
        /// </summary>
        public int NLV { get; private set; }
        /// <summary>
        /// The server's "character code set identifier."
        /// </summary>
        public uint ServerCCSID { get; private set; }

        // TODO: This, amongst other things, should be converted to an enum
        /// <summary>
        /// The password type used by the server.
        /// </summary>
        public byte PasswordLevel { get; private set; }

        PortMapper pm;

        /// <summary>
        /// Creates the server object.
        /// </summary>
        /// <param name="hostname">
        /// The hostname of the server.
        /// </param>
        /// <param name="user">
        /// The username of the account used on the server.
        /// </param>
        /// <param name="password">
        /// The password of the account used on the server.
        /// </param>
        /// <param name="tls">
        /// If TLS should be used when connecting to services.
        /// </param>
        /// <param name="portMapperPort">
        /// The port that the port mapper resides on.
        /// </param>
        /// <param name="portMapperMode">
        /// If the default assumed ports for a service are to be used if the
        /// port mapper is unavailable.
        /// </param>
        /// <param name="nlv">
        /// The "national language version" to use.
        /// </param>
        public Server(string hostname,
            string user,
            string password,
            bool tls = true,
            int portMapperPort = 449,
            PortMapperMode portMapperMode = PortMapperMode.FallbackToAssumed,
            int nlv = 2924)
        {
            if (hostname == null)
                throw new ArgumentNullException(nameof(hostname));
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            Hostname = hostname;
            User = user;
            Password = password;
            TLS = tls;
            PortMapperPort = portMapperPort;
            PortMapperMode = portMapperMode;

            NLV = nlv;
            ServerCCSID = 37; // cromulent assumption

            pm = new PortMapper(Hostname, PortMapperPort);
            SignedOn = false;
        }
        
        internal Connection CreateConnection(Service service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (service.Server != this)
                throw new ArgumentException("The service isn't associated to the server object.", nameof(service));

            Connection c;

            var tc = new TcpClient();
            var port = pm.GetPort(service, TLS, PortMapperMode);
            tc.Connect(Hostname, port);
            if (!tc.Connected)
                throw new Exception("Couldn't connect to the service.");

            var s = tc.GetStream();
            Stream cs;

            if (TLS)
            {
#if DEBUG
                // don't validate TLS
                // TODO: Make this policy controllable
                var ss = new SslStream(s, false, (sender, certificate, chain, errors) => true);
#else
                var ss = new SslStream(s);
#endif
                ss.AuthenticateAsClient(Hostname);
                cs = ss;
            }
            else
            {
                cs = s;
            }

            // it seems for non-signon services, we have to init 
            if (service is SignonService)
            {
                c = new Connection(cs, service, service.ConnectionID, null);
            }
            else
            {
                if (!SignedOn)
                    Signon();

                // exchange seeds
                var seedReq = new RandomSeedExchangeRequest(service.ID);
                seedReq.CorrelationID = service.ConnectionID;
                Packet.WritePacket(cs, seedReq); // we can't use Service
                var seedRes = Packet.ReadPacket<RandomSeedExchangeResponse>(cs);
                if (seedRes.ReturnCode != 0)
                    throw new Exception($"Invalid return code from seed exchange: {seedRes.ReturnCode}");
                // start server request
                var encryptedPassword = PasswordEncrypt.EncryptPassword(User, Password, seedReq.Seed, seedRes.Seed, PasswordLevel);
                var serverReq = new StartServerRequest(User.ToUpperInvariant(), encryptedPassword, service.ID);
                Packet.WritePacket(cs, serverReq);
                var serverRes = Packet.ReadPacket<StartServerResponse>(cs);
                if (serverRes.ReturnCode != 0)
                    throw new Exception($"Invalid return code from server exchange: {serverRes.ReturnCode}");

                c = new Connection(cs, service, serverRes.CorrelationID, serverRes.JobName);
            }

            // now the socket should be authenticated and metadata associated
            return c;
        }

        /// <summary>
        /// Signs onto the server, and if successful, sets
        /// <see cref="SignedOn"/>.
        /// </summary>
        public void Signon()
        {
            var ss = new SignonService(this);
            ss.Connect();
            if (!ss.Connected)
                throw new Exception("Couldn't connect to the signon service.");

            var response = ss.Signon();

            // TODO: Add more information; perhaps just store the signon packet directly?
            ServerCCSID = response.SignonInfoResponse.ServerCCSID;
            PasswordLevel = response.SignonSeedExchangeResponse.PasswordLevel;

            ss.Disconnect();
            SignedOn = true;
        }
    }
}

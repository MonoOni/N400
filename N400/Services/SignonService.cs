﻿using N400.Packets;
using N400.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    internal class SignonService : Service
    {
        // tuple to give to Server
        internal class SignonServiceResponses
        {
            public SignonInfoResponse SignonInfoResponse { get; private set; }
            public SignonSeedExchangeResponse SignonSeedExchangeResponse { get; private set; }

            public SignonServiceResponses(SignonInfoResponse ir, SignonSeedExchangeResponse ser)
            {
                SignonInfoResponse = ir;
                SignonSeedExchangeResponse = ser;
            }
        }

        // TODO: Untangle Service.ID into static entities so this makes more sense
        public const ushort SERVICE_ID = 0xE009;

        public byte[] Seed { get; private set; }

        public SignonService(Server server)
            : base(server, SERVICE_ID, "as-signon", null, 8476, 9476)
        {
        }

        public SignonServiceResponses Signon()
        {
            EnsureInitialized();

            // first, exchange seeds
            var seedReq = new SignonSeedExchangeSeedRequest();
            Seed = seedReq.ClientSeed;
            WritePacket(seedReq);

            // get the response
            var seedRes = ReadPacket<SignonSeedExchangeResponse>();
            if (seedRes.ReturnCode != 0)
                throw new AS400SignonException(seedRes);

            // write out an info request
            var encryptedPassword = PasswordEncrypt.EncryptPassword(Server.User, Server.Password, seedReq.ClientSeed, seedRes.ServerSeed, seedRes.PasswordLevel);
            var infoReq = new SignonInfoRequest(Server.User.ToUpperInvariant(), encryptedPassword, seedRes.ServerLevel);
            WritePacket(infoReq);

            var infoRes = ReadPacket<SignonInfoResponse>();
            if (infoRes.ReturnCode != 0)
                throw new AS400SignonException(infoRes);

            return new SignonServiceResponses(infoRes, seedRes);
        }

        // we don't need this because signon is a special case where we
        // exchange attribs, do auth, then finish what we need to
        protected override bool Initialize() => true;
    }
}
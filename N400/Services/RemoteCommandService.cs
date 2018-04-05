﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Services
{
    internal class RemoteCommandService : Service
    {
        public const ushort SERVICE_ID = 0xE008;

        public RemoteCommandService(Server server)
            : base(server, SERVICE_ID, "as-rmtcmd", null, 8475, 9475)
        {
            // TODO: implement this
            throw new NotImplementedException();
        }

        protected override bool Initialize()
        {
            throw new NotImplementedException();
        }
    }
}

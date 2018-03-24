using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var s = new Server("173.95.177.177", "calvin", password, true, portMapperMode: PortMapperMode.AlwaysUsePortMapper);
            //var s = new Server("173.54.20.170", "calvin", password, false, portMapperMode: PortMapperMode.AlwaysUsePortMapper);
            s.Signon();

            //var remote = new N400.Services.RemoteCommandService(s);
            //remote.GetJobInfo();

            var dq = new DataQueues.DataQueue(s, "CALVIN", "TESTQ");
            dq.Create(8);

            Console.ReadLine();
        }
    }
}

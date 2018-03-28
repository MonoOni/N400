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
            Console.Write("Hostname: ");
            var hostname = Console.ReadLine();
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var s = new Server(hostname, username, password, portMapperMode: PortMapperMode.AlwaysUsePortMapper);
            s.Signon();

            var dq = new DataQueues.DataQueue(s, "LANDO", "TESTQ");
            DataQueues.DataQueueEntry dqe = dq.Peek(0);
            Console.WriteLine(dqe.Data.ToString());


            Console.ReadLine();
        }
    }
}

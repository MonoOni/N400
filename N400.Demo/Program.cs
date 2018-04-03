using System;
using System.Collections.Generic;
using System.IO;
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
            Console.Write("TLS? ");
            var tls = Console.ReadLine().ToLower().StartsWith("y");

            var s = new Server(hostname, username, password, tls, portMapperMode: PortMapperMode.AlwaysUsePortMapper);
            s.Signon();

            //var dq = new DataQueues.DataQueue(s, name: "TESTQ", library: "CALVIN");
            //DataQueues.DataQueueEntry dqe = dq.Peek(0);
            //Console.WriteLine(dqe?.Data?.ToString() ?? "no data");

            var fs = new FileSystem.FileSystem(s);
            const string fileName = "/home/CALVIN/hello.test";
            using (var stream = fs.Open(fileName, create: true))
            {
                //stream.Position = 4;
                //stream.Write(Encoding.Default.GetBytes("TEST"), 0, 4);
                //stream.Position = 0;
                //var buf = new byte[16];
                //stream.Read(buf, 0, 16);
                //Console.WriteLine(Encoding.Default.GetString(buf));

                using (var sw = new BinaryWriter(stream))
                {
                    sw.Write(File.ReadAllText("C:\\tmp\\input.txt"));
                    // not reset?
                    stream.Position = 0;
                    using (var sr = new BinaryReader(stream))
                    {
                        var str = sr.ReadString();
                        Console.WriteLine(str);
                    }
                }
            }
            fs.DeleteFile(fileName);
            var files = fs.List("/home/CALVIN/*");

            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using JSLib;
namespace remotingWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient mclient = new TcpClient();
            mclient.Connect(IPAddress.Loopback, 6553);
            mclient.Client.Blocking = true;
            Stream srcstre = mclient.GetStream();
            Kernel.ExecutionContext = srcstre;
            Kernel executionkernel = new Kernel();
            executionkernel.Initialize();
            BinaryReader mreader = new BinaryReader(srcstre);
            executionkernel.Run(mreader.ReadString());
            Console.WriteLine("Execution complete");
        }
    }
}

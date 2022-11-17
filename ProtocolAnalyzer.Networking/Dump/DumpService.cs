using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalyzer.Networking.Dump
{
    public class DumpService
    {
        public DumpService()
        {

        }

        public string Start()
        {
            Console.WriteLine("=>tcpdump service<=");
            Thread.Sleep(2000);
            return "123";
        }
    }
}

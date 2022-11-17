using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalyzer.Domain.Connections
{
    public class Host
    {
        public int Port { get; private set; }
        public Protocols Protocol { get; private set; }

        public Host(int port, Protocols protocol)
        {
            Port = port;
            Protocol = protocol;
        }
    }
}

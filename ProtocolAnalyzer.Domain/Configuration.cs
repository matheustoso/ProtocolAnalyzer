using ProtocolAnalyzer.Domain.Connections;

namespace ProtocolAnalyzer.Domain
{
    public class Configuration
    {
        public IEnumerable<Host> Hosts { get; set; }
        public IEnumerable<Peer> Peers { get; set; }

        public Configuration(IEnumerable<Host> hosts, IEnumerable<Peer> peers)
        {
            Hosts = hosts;
            Peers = peers;
        }

    }
}

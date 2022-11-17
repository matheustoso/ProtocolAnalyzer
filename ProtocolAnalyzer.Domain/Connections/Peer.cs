using System.Text.Json.Serialization;

namespace ProtocolAnalyzer.Domain.Connections
{
    public class Peer : Host
    {
        public Guid Id { get; private set; }
        public string IP { get; private set; }
        public Peer(string ip, int port, Protocols protocol) : base(port, protocol)
        {
            Id = Guid.NewGuid();
            IP = ip;
        }

        [JsonConstructor]
        public Peer(Guid id, string ip, int port, Protocols protocol) : base(port, protocol)
        {
            Id = id;
            IP = ip;
        }
    }
}

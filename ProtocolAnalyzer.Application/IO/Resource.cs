using System.Text;

namespace ProtocolAnalyzer.Application.IO
{
    public sealed class Resource
    {
        public static readonly Resource Title = new("IETF QUIC Protocol Analyzer v1");
        public static readonly Resource InvalidPort = new("Invalid port number. Try again.");
        public static readonly Resource InvalidIP = new("Invalid IP. Try again.");
        public static readonly Resource InvalidProtocol = new("Invalid Protocol. Try again.");
        public static readonly Resource InvalidPeerId = new("Invalid Peer ID. Try again.");
        public static readonly Resource PeerIPRequest = new("Type the peer`s IP:");
        public static readonly Resource PeerPortRequest = new("Type the peer`s Port:");
        public static readonly Resource PeerProtocolRequest = new("Type the peer`s Protocol:");
        public static readonly Resource RemovePeerRequest = new("Type the ID of the peer you wish to remove:");
        public static readonly Resource HostPortRequest = new("Type the host`s new Port for the selected protocol:");
        public static readonly Resource UnconfiguredHost = new("No port configured for this protocol.");
        public static readonly Resource Menu = new("""
            ________________________________________________________________________
                0. Exit
                1. Peers
                2. Host
                3. TCPdump
            """);
        public static readonly Resource AddPeerMenu = new("""
            ________________________________________________________________________
                Choose Protocol
                UDP
                TCP
                QUIC

                0. Return
            """);
        public static readonly Resource AnalysisMenu = new("""
            ________________________________________________________________________
                0. Return
                1. Request TCPdump
                2. Start Listening
            """);
        public static readonly Resource EditHostMenu = AddPeerMenu;
        public static Resource SelectHostMenu => BuildHostMenu(true);
        public static Resource SelectPeerMenu => BuildPeerMenu(true);
        public static Resource PeerMenu => BuildPeerMenu();
        public static Resource HostMenu => BuildHostMenu();

        private static Resource BuildPeerMenu(bool select = false)
        {
            var peers = FileStorage.GetPeers();
            var peerMenu = new StringBuilder("________________________________________________________________________\n");
            peerMenu.AppendFormat("      {0,-8}\t\t{1,-15}\t\t{2,5}", "Protocol", "IP", "Port");
            peerMenu.AppendLine();
            foreach (var peer in peers.Select((p, i) => new { Value = p, Index = i }))
            {
                peerMenu.AppendFormat("{0,4}. {1,-8}\t\t{2,-15}\t\t{3,5}", peer.Index + 1, peer.Value.Protocol, peer.Value.IP, peer.Value.Port); ;
                peerMenu.AppendLine();
            }
            peerMenu.AppendLine();
            peerMenu.AppendFormat(select ? "Type the ID of the peer you wish to run TCPdump on:" : "{0,10}\t{1,11}\t{2,14}", "0. Return", "1. Add Peer", "2. Remove Peer");
            return new Resource(peerMenu.ToString());
        }

        private static Resource BuildHostMenu(bool select = false)
        {
            var hosts = FileStorage.GetHosts();
            var hostMenu = new StringBuilder("________________________________________________________________________\n");
            hostMenu.AppendFormat("{0,-8}\t\t{1,5}", "Protocol", "Port");
            hostMenu.AppendLine();
            foreach (var host in hosts)
            {
                hostMenu.AppendFormat("{0,-8}\t\t{1,5}", host.Protocol, host.Port); ;
                hostMenu.AppendLine();
            }
            hostMenu.AppendLine();
            hostMenu.AppendFormat(select ? "Type the protocol you wish to listen to:" : "{0,10}\t{1,7}", "0. Return", "1. Edit");
            return new Resource(hostMenu.ToString());
        }

        public string Value { get; private set; }

        private Resource(string value)
        {
            Value = value;
        }
    }
}

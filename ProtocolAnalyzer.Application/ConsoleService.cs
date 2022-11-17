using System.Text.RegularExpressions;
using ProtocolAnalyzer.Application.IO;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking;

namespace ProtocolAnalyzer.Application
{
    public partial class ConsoleService
    {
        private NetworkingService _networkingService;
        private DataService _dataService;
        public ConsoleService()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = Resource.Title.Value;
            _networkingService = new(new(FileStorage.GetHosts(), FileStorage.GetPeers()));
            _dataService = new();
        }

        public void DisplayMenu()
        {
            while (true)
            {
                Console.WriteLine(Resource.Menu.Value);
                var selection = Console.ReadLine();
                switch (selection)
                {
                    case "0":
                        return;
                    case "1":
                        DisplayPeerMenu();
                        break;
                    case "2":
                        DisplayHostMenu();
                        break;
                    case "3":
                        UpdateNetworking();
                        DisplayAnalysisMenu();
                        break;
                    default:
                        break;
                }
            }
        }

        private void DisplayPeerMenu()
        {
            while (true)
            {
                Console.WriteLine(Resource.PeerMenu.Value);
                var selection = Console.ReadLine();
                switch (selection)
                {
                    case "0":
                        return;
                    case "1":
                        HandleAddPeer();
                        break;
                    case "2":
                        HandleRemovePeer();
                        break;
                    default:
                        break;
                }
            }
        }

        private void HandleAddPeer()
        {
            Console.WriteLine(Resource.PeerProtocolRequest.Value);
            Protocols protocol = ParseProtocol();

            Console.WriteLine(Resource.PeerIPRequest.Value);
            string ip = ParseIp();

            Console.WriteLine(Resource.PeerPortRequest.Value);
            int port = ParsePort();

            FileStorage.Save(new Peer(ip, port, protocol));
        }

        private void HandleRemovePeer()
        {
            var peers = FileStorage.GetPeers();
            if (!peers.Any()) return;

            int peerId;
            Console.WriteLine(Resource.RemovePeerRequest.Value);
            while (!int.TryParse(Console.ReadLine(), out peerId) || peerId < 1 || peerId > peers.Count())
            {
                Console.WriteLine(Resource.InvalidPeerId.Value);
            }

            FileStorage.Delete(peers.ElementAt(peerId - 1).Id);
        }

        private void DisplayHostMenu()
        {
            while (true)
            {
                Console.WriteLine(Resource.HostMenu.Value);
                var selection = Console.ReadLine();
                switch (selection)
                {
                    case "0":
                        return;
                    case "1":
                        HandleEditHost();
                        break;
                    default:
                        break;
                }
            }
        }

        private void HandleEditHost()
        {
            Console.WriteLine(Resource.EditHostMenu.Value);
            var protocol = ParseProtocol();

            Console.WriteLine(Resource.HostPortRequest.Value);
            var port = ParsePort();

            FileStorage.Delete(protocol);
            FileStorage.Save(new Host(port, protocol));
        }

        private void DisplayAnalysisMenu()
        {
            while (true)
            {
                Console.WriteLine(Resource.AnalysisMenu.Value);
                var selection = Console.ReadLine();
                string data;
                switch (selection)
                {
                    case "0":
                        return;
                    case "1":
                        HandleStartTCPDump();
                        break;
                    case "2":
                        HandleStartListening();
                        break;
                    default:
                        break;
                }
            }
        }

        private void HandleStartTCPDump()
        {
            var peers = FileStorage.GetPeers();
            if (!peers.Any()) return;

            int peerId;
            Console.WriteLine(Resource.SelectPeerMenu.Value);
            while (!int.TryParse(Console.ReadLine(), out peerId) || peerId < 1 || peerId > peers.Count())
            {
                Console.WriteLine(Resource.InvalidPeerId.Value);
            }

            var peer = peers.ElementAt(peerId - 1);
            var data = _networkingService.Analyze(peer);
            ParseData(data);
        }

        private void HandleStartListening()
        {
            var hosts = FileStorage.GetHosts();
            if (!hosts.Any()) return;

            Console.WriteLine(Resource.SelectHostMenu.Value);
            var protocol = ParseProtocol();

            var host = hosts.FirstOrDefault(h => h.Protocol == protocol);
            var data = _networkingService.ListenOn(host);
            ParseData(data);
        }

        private void ParseData(Message data)
        {
            var parsedData = _dataService.Parse(data);
            Console.WriteLine(parsedData);
        }

        private static Protocols ParseProtocol()
        {
            Protocols protocol;
            while (!Enum.TryParse(Console.ReadLine(), true, out protocol))
            {
                Console.WriteLine(Resource.InvalidProtocol.Value);
            }
            return protocol;
        }

        private static string ParseIp()
        {
            var ip = Console.ReadLine()!;
            while (!IpRegex().IsMatch(ip))
            {
                Console.WriteLine(Resource.InvalidIP.Value);
                ip = Console.ReadLine()!;
            }
            return ip;
        }

        private static int ParsePort()
        {
            int port;
            while (!int.TryParse(Console.ReadLine(), out port) || port > 65535 || port < 1024)
            {
                Console.WriteLine(Resource.InvalidPort.Value);
            }
            return port;
        }

        private void UpdateNetworking()
        {
            _networkingService.UpdateConfiguration(new Configuration(FileStorage.GetHosts(), FileStorage.GetPeers()));
        }

        [GeneratedRegex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$")]
        private static partial Regex IpRegex();
    }
}

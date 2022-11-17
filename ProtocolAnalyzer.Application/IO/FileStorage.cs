using System.Text.Json;
using ProtocolAnalyzer.Domain.Connections;

namespace ProtocolAnalyzer.Application.IO
{
    public static class FileStorage
    {
        public static readonly string RootPath = Path.Combine(AppContext.BaseDirectory, "resources");
        public static readonly string HostsPath = Path.Combine(RootPath, "hosts.txt");
        public static readonly string PeersPath = Path.Combine(RootPath, "peers.txt");

        public static void Setup()
        {
            Directory.CreateDirectory(RootPath);
            if (!File.Exists(HostsPath)) File.Create(HostsPath);
            if (!File.Exists(PeersPath)) File.Create(PeersPath);
        }

        public static void Save(Peer peer)
        {
            File.AppendAllLines(PeersPath, new[] { JsonSerializer.Serialize(peer) });
        }

        public static void Save(Host host)
        {
            File.AppendAllLines(HostsPath, new[] { JsonSerializer.Serialize(host) });
        }

        public static void Delete(Guid peerId)
        {
            File.WriteAllLines(PeersPath, GetPeers().Where(p => !p.Id.Equals(peerId)).Select(p => JsonSerializer.Serialize(p)));
        }

        public static void Delete(Protocols hostProtocol)
        {
            File.WriteAllLines(HostsPath, GetHosts().Where(h => h.Protocol != hostProtocol).Select(h => JsonSerializer.Serialize(h)));
        }

        public static IEnumerable<Host> GetHosts()
        {
            return File.ReadAllLines(HostsPath).Select(l => JsonSerializer.Deserialize<Host>(l)!).OrderBy(h => h.Protocol);
        }

        public static IEnumerable<Peer> GetPeers()
        {
            return File.ReadAllLines(PeersPath).Select(l => JsonSerializer.Deserialize<Peer>(l)!).OrderBy(p => p.IP).ThenBy(p => p.Protocol).ThenByDescending(p => p.Port);
        }
    }
}

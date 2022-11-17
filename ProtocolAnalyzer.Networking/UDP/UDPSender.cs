using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;

namespace ProtocolAnalyzer.Networking.UDP
{
    public class UDPSender : ISender
    {
        private readonly UdpClient Client;
        private readonly Peer Peer;

        public UDPSender(Peer peer)
        {
            Peer = peer;
            Client = new(Peer.IP, Peer.Port);
        }

        public Message Receive()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(Peer.IP), Peer.Port);
            var bytes = Client.Receive(ref endpoint);

            var data = Encoding.ASCII.GetString(bytes);
            return new Message(data);
        }

        public void Send(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);

            Client.Send(bytes, bytes.Length);
        }

        public void Stop()
        {
            Client.Close();
        }
    }
}

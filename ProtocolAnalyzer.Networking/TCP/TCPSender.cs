using System.Net.Sockets;
using System.Text;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;
using ProtocolAnalyzer.Networking.UDP;

namespace ProtocolAnalyzer.Networking.TCP
{
    public class TCPSender : ISender
    {
        private readonly TcpClient Client;
        private readonly Peer Peer;

        public TCPSender(Peer peer)
        {
            Peer = peer;
            Client = new TcpClient(Peer.IP, Peer.Port);
        }

        public Message Receive()
        {
            var bytes = new byte[256];
            var stream = Client.GetStream();

            StringBuilder data = new();

            while (stream.DataAvailable)
            {
                int i = stream.Read(bytes, 0, bytes.Length);
                data.Append(Encoding.ASCII.GetString(bytes, 0, i));
            }

            return new Message(data.ToString());
        }

        public void Send(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);
            var stream = Client.GetStream();

            stream.Write(bytes, 0, bytes.Length);
        }

        public void Stop()
        {
            Client.Close();
        }
    }
}

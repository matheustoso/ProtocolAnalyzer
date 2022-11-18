using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;

namespace ProtocolAnalyzer.Networking.TCP
{
    public class TCPListener : IListener
    {
        private readonly TcpListener Listener;
        private readonly Host Host;
        private readonly TcpClient Client;

        public TCPListener(Host host)
        {
            Host = host;
            Listener = new(IPAddress.Any, Host.Port);
            Listener.Start();
            Client = Listener.AcceptTcpClient();
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

            return new Message(data.ToString()).SetEndpoint(Client.Client.RemoteEndPoint);
        }

        public void Reply(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);
            var stream = Client.GetStream();

            stream.Write(bytes, 0, bytes.Length);
        }

        public void Stop()
        {
            Listener.Stop();
            Client.Close();
        }
    }
}

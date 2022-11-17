using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ProtocolAnalyzer.Networking.Interfaces;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Domain;

namespace ProtocolAnalyzer.Networking.UDP
{
    public class UDPListener : IListener
    {
        private readonly Host Host;
        private readonly UdpClient Client;
        private IPEndPoint Endpoint;

        public UDPListener(Host host)
        {
            Host = host;
            Client = new(Host.Port);
            Endpoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public Message Receive()
        {
            var bytes = Client.Receive(ref Endpoint);

            var data = Encoding.ASCII.GetString(bytes);
            return new Message(data).SetEndpoint(Endpoint);
        }

        public void Reply(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);

            Client.Send(bytes, bytes.Length, Endpoint);
        }

        public void Stop()
        {
            Client.Close();
        }
    }
}

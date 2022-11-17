using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;

namespace ProtocolAnalyzer.Networking.QUIC
{
    public class QUICSender : ISender
    {
        private readonly QuicConnection Connection;
        private readonly QuicStream Stream;
        private readonly Peer Peer;

        public QUICSender(Peer peer)
        {
            Peer = peer;
            var options = new QuicClientConnectionOptions
            {
                ClientAuthenticationOptions = new()
                {
                    AllowRenegotiation = true,
                    ApplicationProtocols = new()
                    {
                        SslApplicationProtocol.Http3
                    },
                    RemoteCertificateValidationCallback = ValidateServerCertificate,
                    TargetHost = Peer.IP+Peer.Port,
                },
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(Peer.IP), Peer.Port),
                DefaultStreamErrorCode = 4,
                DefaultCloseErrorCode = 5,
                IdleTimeout = TimeSpan.FromSeconds(20),
                MaxInboundBidirectionalStreams = 5,
                MaxInboundUnidirectionalStreams = 5
            };

            var connectionTask = Task.Run(() => QuicConnection.ConnectAsync(options));
            connectionTask.Wait();
            Connection = connectionTask.Result.Result;

            var streamTask = Task.Run(() => Connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional));
            streamTask.Wait();
            Stream = streamTask.Result.Result;
        }

        public Message Receive()
        {
            var bytes = new byte[256];

            StringBuilder data = new();

            while (true)
            {
                var receiveTask = Task.Run(() => Stream.Read(bytes, 0, bytes.Length));
                receiveTask.Wait();
                int i = receiveTask.Result;
                if (i == 0) break;
                data.Append(Encoding.ASCII.GetString(bytes, 0, i));
                if (Encoding.ASCII.GetString(bytes, 0, i).Length > 0) Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, i));
                Thread.Sleep(100);
            }

            return new Message(data.ToString());
        }

        public void Send(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);

            var sendTask = Task.Run(() => Stream.WriteAsync(bytes, false));
            sendTask.Wait();

            Stream.CompleteWrites();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        public void Stop()
        {
            Stream.Dispose();
            Connection.DisposeAsync();
        }
    }
}

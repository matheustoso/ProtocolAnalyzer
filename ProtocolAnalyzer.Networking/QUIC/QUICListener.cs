using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;

namespace ProtocolAnalyzer.Networking.QUIC
{
    public class QUICListener : IListener
    {
        private readonly QuicListener Listener;
        private readonly QuicConnection Connection;
        private readonly QuicStream Stream;
        private readonly Host Host;


        public QUICListener(Host host)
        {
            Host = host;
            var options = new QuicListenerOptions
            {
                ListenEndPoint = new IPEndPoint(IPAddress.Any, Host.Port),
                ApplicationProtocols = new()
                {
                    SslApplicationProtocol.Http3
                },
                ConnectionOptionsCallback = GetConnectionOptions
            };

            var listenerTask = Task.Run(() => QuicListener.ListenAsync(options));
            listenerTask.Wait();
            Listener = listenerTask.Result.Result;

            var connectionTask = Task.Run(() => Listener.AcceptConnectionAsync());
            connectionTask.Wait();  
            Connection = connectionTask.Result.Result;

            Task.Run(Listener.DisposeAsync);

            var streamTask = Task.Run(() => Connection.AcceptInboundStreamAsync());
            streamTask.Wait();
            Stream = streamTask.Result.Result;
        }

        private ValueTask<QuicServerConnectionOptions> GetConnectionOptions(QuicConnection quicConnection, SslClientHelloInfo sslClientHelloInfo, CancellationToken cancellationToken) 
        {
            return new ValueTask<QuicServerConnectionOptions>(new QuicServerConnectionOptions
            {
                ServerAuthenticationOptions = new()
                {
                    AllowRenegotiation = true,
                    ApplicationProtocols = new()
                    {
                        SslApplicationProtocol.Http3
                    },
                    RemoteCertificateValidationCallback = ValidateServerCertificate,
                    ServerCertificate = GenerateSelfSignedCertificate(),
                    ClientCertificateRequired = false,
                },
                DefaultStreamErrorCode = 4,
                DefaultCloseErrorCode = 5,
                IdleTimeout = TimeSpan.FromSeconds(20),
                MaxInboundBidirectionalStreams = 5,
                MaxInboundUnidirectionalStreams = 5
            });
        }

        public Message Receive()
        {
            //Change to return only when data read : i > 0
            var bytes = new byte[256];

            StringBuilder data = new();

            while (true)
            {
                var receiveTask = Task.Run(() => Stream.ReadAsync(bytes, 0, bytes.Length));
                receiveTask.Wait();
                int i = receiveTask.Result;
                if (i == 0) break;
                data.Append(Encoding.ASCII.GetString(bytes, 0, i));
                Console.WriteLine(i);
                Thread.Sleep(100);
            }

            return new Message(data.ToString()).SetEndpoint(Connection.RemoteEndPoint);
        }

        public void Reply(Message message)
        {
            var bytes = Encoding.ASCII.GetBytes(message.Data);

            var sendTask = Task.Run(() => Stream.WriteAsync(bytes, 0, bytes.Length));
            sendTask.Wait();

            Stream.CompleteWrites();
        }

        public void Stop()
        {
            Stream.Dispose();
            Connection.DisposeAsync();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        private static X509Certificate2 GenerateSelfSignedCertificate()
        {
            string secp256r1Oid = "1.2.840.10045.3.1.7";  //oid for prime256v1(7)  other identifier: secp256r1

            string subjectName = "Self-Signed-Cert-Example";

            var ecdsa = ECDsa.Create(ECCurve.CreateFromValue(secp256r1Oid));

            var certRequest = new CertificateRequest($"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);

            //add extensions to the request (just as an example)
            //add keyUsage
            certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

            X509Certificate2 generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10)); // generate the cert and sign!

            X509Certificate2 pfxGeneratedCert = new X509Certificate2(generatedCert.Export(X509ContentType.Pfx)); //has to be turned into pfx or Windows at least throws a security credentials not found during sslStream.connectAsClient or HttpClient request...

            return pfxGeneratedCert;
        }
    }
}

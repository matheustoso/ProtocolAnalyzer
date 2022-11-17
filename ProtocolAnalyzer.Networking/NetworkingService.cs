using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;
using ProtocolAnalyzer.Networking.Interfaces;
using ProtocolAnalyzer.Networking.UDP;
using ProtocolAnalyzer.Networking.TCP;
using ProtocolAnalyzer.Networking.QUIC;
using ProtocolAnalyzer.Networking.Dump;
using System.Reflection;

namespace ProtocolAnalyzer.Networking
{
    public class NetworkingService
    {
        public Configuration Configuration { get; private set; }
        private readonly DumpService _dumpService;
        public NetworkingService(Configuration configuration)
        {
            UpdateConfiguration(configuration);
            _dumpService = new DumpService();
        }

        public void UpdateConfiguration(Configuration configuration)
        {
            Configuration = configuration;
        }

        public Message ListenOn(Host host)
        {
            Message data;
            var listener = GetListener(host);

            var message = listener.Receive();

            if (message.Data.Equals(Message.START_TCPDUMP.Data, StringComparison.InvariantCultureIgnoreCase))
            {
                var dumpTask = Task.Factory.StartNew(_dumpService.Start, TaskCreationOptions.LongRunning);

                listener.Reply(Message.OK);//not received OK

                while (!dumpTask.IsCompleted) Console.WriteLine(listener.Receive().Data);//breaks as soon as dumptask completes, prints blanks

                data = new Message("TCPDUMP-DATA: " + dumpTask.Result);

                Console.WriteLine("replying data");
                listener.Reply(data);//where does the data go
                
                foreach (var peer in Configuration.Peers.Where(p => p.Protocol == host.Protocol && !p.IP.Equals(message.EndPoint?.Address.ToString()))) Task.Run(() => RelayData(data, peer));
            }
            else if (message.Data.Equals(Message.INCOMING_DATA.Data, StringComparison.InvariantCultureIgnoreCase))
            {
                listener.Reply(Message.OK);
                data = listener.Receive();
                listener.Reply(Message.OK);
            }
            else
            {
                data = message;
            }

            Thread.Sleep(1000);
            listener.Stop();
            return data;
        }

        public Message Analyze(Peer peer)
        {
            Message data;
            var sender = GetSender(peer);

            sender.Send(Message.START_TCPDUMP);//prints

            Console.WriteLine(sender.Receive().Data);//not received OK

            var receiveTask = Task.Run(sender.Receive);//instantly completes because quic is fucked

            while (!receiveTask.IsCompleted)//wont work
            {
                Console.WriteLine("sent dump");
                sender.Send(new Message(Guid.NewGuid().ToString()));
            }

            data = receiveTask.Result;

            while(!data.Data.Contains("TCPDUMP-DATA:")) //stays here forever
            {
                data = sender.Receive();
            }

            Thread.Sleep(1000);
            sender.Stop();
            return data;
        }

        private void RelayData(Message message, Peer peer)
        {
            var sender = GetSender(peer);
            sender.Send(Message.INCOMING_DATA);
            if (sender.Receive().Data != Message.OK.Data) return;
            sender.Send(message);
            sender.Receive();
            sender.Stop();
        }

        private ISender GetSender(Peer peer)
        {
            switch (peer.Protocol)
            {
                case Protocols.UDP:
                    return new UDPSender(peer);
                case Protocols.TCP:
                    return new TCPSender(peer);
                case Protocols.QUIC:
                default:
                    return new QUICSender(peer);
            }
        }

        private IListener GetListener(Host host)
        {
            switch (host.Protocol)
            {
                case Protocols.UDP:
                    return new UDPListener(host);
                case Protocols.TCP:
                    return new TCPListener(host);
                case Protocols.QUIC:
                default:
                    return new QUICListener(host);
            }
        }
    }
}

using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;

namespace ProtocolAnalyzer.Networking.Interfaces
{
    public interface IListener
    {
        public Message Receive();
        public void Reply(Message message);
        public void Stop();
    }
}

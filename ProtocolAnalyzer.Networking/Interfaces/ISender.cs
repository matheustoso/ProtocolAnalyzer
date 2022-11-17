using ProtocolAnalyzer.Domain;
using ProtocolAnalyzer.Domain.Connections;

namespace ProtocolAnalyzer.Networking.Interfaces
{
    public interface ISender
    {
        public Message Receive();
        public void Send(Message message);
        public void Stop();
    }
}

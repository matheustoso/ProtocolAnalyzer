using System.Net;

namespace ProtocolAnalyzer.Domain
{
    public class Message
    {
        public static readonly Message OK = new("OK"); 
        public static readonly Message START_TCPDUMP = new("START"); 
        public static readonly Message INCOMING_DATA = new("INCOMING");

        public string Data { get; private set; }
        public IPEndPoint? EndPoint { get; private set; }

        public Message(string data)
        {
            Data = data;
        }

        public Message SetEndpoint(EndPoint endpoint)
        {
            EndPoint = (IPEndPoint)endpoint;
            return this;
        }
    }
}

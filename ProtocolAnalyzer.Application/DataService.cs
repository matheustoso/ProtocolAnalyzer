using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtocolAnalyzer.Domain;

namespace ProtocolAnalyzer.Application
{
    public class DataService
    {
        public DataService()
        {

        }

        public string Parse(Message message)
        {
            return message.Data;
        }
    }
}

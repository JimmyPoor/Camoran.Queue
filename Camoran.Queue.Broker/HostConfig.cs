using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker
{
    public class HostConfig
    {
        public bool ServerWithAnyIPAddress { get; set; }
        public int  ProducePort { get; set; }
        public string ProduceAddress { get; set; }

        public  int ConsumerPort { get; set; }

        public string ConsumerAddress { get; set; }
        public Dictionary<string, RemoteClientInfo> AllowedClients { get; set; }
    }

    public class RemoteClientInfo
    {
         public string Address { get; set; }
         public int Port { get; set; }
    }
}

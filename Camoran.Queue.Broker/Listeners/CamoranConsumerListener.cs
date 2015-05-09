using Camoran.Queue.Client.Consumer;
using System;
using System.Configuration;

namespace Camoran.Queue.Broker.Listeners
{
    public class CamoranConsumerListener : CamoranClientListener<ConsumerRequest,ConsumerResponse>
    {
        private static string consumer_address = ConfigurationManager.AppSettings["BrokerAddress"].ToString();
        private static int consumer_port = Convert.ToInt32(ConfigurationManager.AppSettings["BorkerPortForConsumer"]);

        public CamoranConsumerListener()
            : base(consumer_address, consumer_port)
        {
        }

        public CamoranConsumerListener(string address,int port)
            :base(address,port)
        {
        
        }
    }
}

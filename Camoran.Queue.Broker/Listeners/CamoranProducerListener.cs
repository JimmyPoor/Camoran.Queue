using Camoran.Queue.Client.Producer;
using System;
using System.Configuration;

namespace Camoran.Queue.Broker.Listeners
{
    public class CamoranProducerListener : CamoranClientListener<ProducerRequest, ProducerResponse>
    {
        private static string producer_address = ConfigurationManager.AppSettings["BrokerAddress"].ToString();
        private static int producer_port = Convert.ToInt32(ConfigurationManager.AppSettings["BorkerPortForProducer"]);
        public CamoranProducerListener()
            : base(producer_address, producer_port)
        {

        }

        public CamoranProducerListener(string address, int port)
            : base(address, port)
        {

        }
    }
}

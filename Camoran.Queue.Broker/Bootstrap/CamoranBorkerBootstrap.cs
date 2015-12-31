using Camoran.Queue.Broker.Brokers;
using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Util.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Bootstrap
{
    public class CamoranBorkerBootstrap
    {
        ICamoranBrokerMachine brokerMachine = null;
        HostConfig config=null;
        ISerializeProcessor serializer = null;
        public CamoranBorkerBootstrap()
        {

        }


        public CamoranBorkerBootstrap CreateBrokerMachine(BrokerConfig conifg)
        {
            brokerMachine = new CamoranBrokerMachine();
            return this;
        }

        public CamoranBorkerBootstrap BindSession(ICamoranBrokerSession session)
        {
            brokerMachine.RegistBrokerSession(session);
            return this;
        }

        public CamoranBorkerBootstrap BuildHostConfig(HostConfig config)
        {
            this.config = config;
            return this;
        }

        public CamoranBorkerBootstrap BindLisenter()
        {
            if (config == null) throw new NullReferenceException("need bind config ");
            brokerMachine.InitialClientListener(config);
            return this; 
        }


        public CamoranBorkerBootstrap SetSerializer(ISerializeProcessor serializer)
        {
            return this;
        }


        public void Start()
        {
            brokerMachine.Start();
        }


    }
}

using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.UnitTest;
using Camoran.Queue.UnitTest.Broker.Machine;
using Camoran.Queue.UnitTest.Client.Consumer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConsumerTest test = new ConsumerTest();
            //test.Start_Consume_Whole_Action_with_Mulit_Thread_Diff_Consumer_Test();
            //Console.ReadLine();

            CamoranBrokerMachineTest test = new CamoranBrokerMachineTest();
            test.StartMachineTset();
            Console.Read();
        }

        private static readonly string _address = "127.0.0.1";//"10.197.218.137";// ConfigurationManager.AppSettings["BrokerAddress"];
        private static readonly int _port = 8080; //Convert.ToInt32(ConfigurationManager.AppSettings["BorkerPort"]);

        private static CamoranConsumer CreateConsumer(Guid id)
        {
            ClientConfig config = new ClientConfig { Address = _address, Port = _port };
            var inner = new Client_byHelios<ConsumerRequest, ConsumerResponse>(id, config);
            var producer = new CamoranConsumer(id, config, inner);
            return producer;
        }
    }

}

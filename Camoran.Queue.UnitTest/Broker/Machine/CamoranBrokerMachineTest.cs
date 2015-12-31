using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Camoran.Queue.Broker.Brokers;
using System.Threading.Tasks;
using Camoran.Queue.Core.Queue;
using System.Collections.Generic;
using System.Linq;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Broker.Queue;
using Camoran.Queue.Util;
using Camoran.Queue.Broker;

namespace Camoran.Queue.UnitTest.Broker.Machine
{
    [TestClass]
    public class CamoranBrokerMachineTest
    {
        CamoranBrokerMachine machine;
        HostConfig config = new HostConfig
        {
            ServerWithAnyIPAddress = false,
            ConsumerPort =8080,
            ProducePort = 8081,
            ConsumerAddress = "127.0.0.1",
            ProduceAddress="127.0.0.1"
        };
        [TestMethod]
        public void InitialMachineTest()
        {
            machine = new CamoranBrokerMachine();
            //Assert.AreNotEqual(machine.MessageStore,null);
            Assert.AreNotEqual(machine.Session.QueueService.QueueProcessor, null);
            Assert.AreNotEqual(machine.Session, null);
            Assert.AreNotEqual(machine.Session.QueueService.TopicQueues, null);
        }

        [TestMethod]
        public void StartMachineTset()
        {
            machine = new CamoranBrokerMachine()
            .RegistBrokerSession(new CamoranBrokerSession())
            .InitialClientListener(config) as CamoranBrokerMachine;
            machine.Start();
        }
        [TestMethod]
        public void StopMachineTest()
        {
          Task startTask=Task.Run(()=>StartMachineTset());
          Task stopTask = Task.Run(()=>
             {
                
                 while (true)
                 {
                     System.Threading.Thread.Sleep(5000);
                     if (machine != null)
                     {
                         machine.Stop();
                         break;
                     }
                 }
             }
              );
          Task.WaitAll(startTask,stopTask);

          Assert.IsTrue(true);

        }
    }
}

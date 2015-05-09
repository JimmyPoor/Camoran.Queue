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

namespace Camoran.Queue.UnitTest.Broker.Machine
{
    [TestClass]
    public class CamoranBrokerMachineTest
    {
        CamoranBrokerMachine machine;

        [TestMethod]
        public void InitialMachineTest()
        {
            machine = new CamoranBrokerMachine();
            //Assert.AreNotEqual(machine.MessageStore,null);
            Assert.AreNotEqual(machine.QueueProcessor, null);
            Assert.AreNotEqual(machine.Session, null);
            Assert.AreNotEqual(machine.Session.TopicQueues, null);
        }

        [TestMethod]
        public void StartMachineTset()
        {
            machine = new CamoranBrokerMachine()
            .RegistBrokerSession(new CamoranBrokerSession())
            .RegistProcessor(new CamoranMQProcessor())
            .InitialClientListener();
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

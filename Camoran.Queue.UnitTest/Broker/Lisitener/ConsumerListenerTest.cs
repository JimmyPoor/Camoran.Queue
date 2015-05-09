using Camoran.Queue.Broker.Listeners;
using Camoran.Queue.Client.Consumer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.UnitTest.Broker.LisitenerTest
{
    [TestClass]
    public class ConsumerListenerTest
    {
        CamoranConsumerListener _listener;
        //string topic="listener";
        ConsumerResponse demoRegisterResponse;
        ConsumerResponse demoConsumeResponse;

        ConsumerRequest registerRequest=null;
        public ConsumerListenerTest()
        {
            _listener = new CamoranConsumerListener();

            demoRegisterResponse = new ConsumerResponse("registTopic",null, Guid.Empty);
            demoConsumeResponse = new ConsumerResponse("consumeTopic",null, Guid.Empty);

        }
        [TestMethod]
        public void BindReceiveEventTest()
        {
            string requestType = ConsumerRequestType.consume.ToString();
           //_listener.ReceiveEvents.GetOrAdd(ConsumerRequestType.rigister,(req)=>demoRegisterResponse);
            _listener.ReceiveEvents.GetOrAdd(requestType, (req) => demoConsumeResponse);

           Assert.AreEqual(_listener.ReceiveEvents.Count,2);
          // Assert.AreEqual(_listener.ReceiveEvents.Keys.ToArray()[0],ConsumerRequestType.rigister);
           Assert.AreEqual(_listener.ReceiveEvents.Keys.ToArray()[1], ConsumerRequestType.consume);
           Assert.AreEqual(_listener.ReceiveEvents.Values.First().Invoke(registerRequest), demoRegisterResponse);
        }


        [TestMethod]
        public void InovkeRegisterEventTest() 
        {
            //BindRegisterEvent();
            StartListener();
        }



        //private void BindRegisterEvent() 
        //{

        //    _listener.ReceiveEvents.Clear();
        //    _listener.ReceiveEvents.GetOrAdd(ConsumerRequestType.rigister, (req) =>
        //    {
        //        Assert.AreNotEqual(null, req);
        //        Assert.AreEqual(ConsumerRequestType.rigister, req.RequestType);
        //        Assert.AreEqual(req.Topic, "consumerTopic");
        //        demoRegisterResponse.SenderId = req.SenderId;
        //        return demoRegisterResponse;
        //    });
        //}

        private void StartListener()
        {
            _listener.StartListen();
        }
    }
}

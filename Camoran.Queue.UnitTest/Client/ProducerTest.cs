using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Camoran.Queue.Client.Producer;
using System.Configuration;
using Camoran.Queue.Client;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace Camoran.Queue.UnitTest.Client
{
    [TestClass]
    public class ProducerTest
    {
        string topic = "topic";
        private readonly string _address = TestConfig.BrokerAddress;// ConfigurationManager.AppSettings["BrokerAddress"];
        private readonly int _port = TestConfig.ProducerListenerPort;// Convert.ToInt32(ConfigurationManager.AppSettings["BorkerPort"]);
        public ProducerTest()
        {

        }
        [TestMethod]
        public void Send_Message_to_Queue_with_Single_Thread_Same_Producer()
        {
            var producer = CreateProducer();
            producer.ConnectToServer();
            producer.Start();
            var response = producer.SendRequest(new ProducerRequest(topic + "1", null, producer.ClientId, ProducerRequestType.send));
            Console.Read();
            // var response2 = producer.SendRequest(new ProducerRequest(topic + "2", null, producer.ClientId, ProducerRequestType.send));
            //Console.Read();
            //Assert.AreEqual(response.Body, null);
            //Assert.AreEqual(response.SendSuccess, true);
        }

        static object lockobj = new object();
        [TestMethod]
        public void Send_Message_to_Queue_with_Multi_Thread_Same_Producer_Diff_Topic()
        {
            List<ProducerResponse> responses = new List<ProducerResponse>();
            var producer = CreateProducer();
            producer.ConnectToServer();
            Parallel.For(0, TestConfig.Producer_Send_Count, (i) =>
            {
                i = Interlocked.Increment(ref i);

                lock (lockobj)
                {
                    //var producer = CreateProducer();
                    //producer.ConnectToServer();
                    Console.WriteLine(i);
                    var body = Encoding.UTF8.GetBytes("Sender:" + i);
                    var response = producer.SendRequest(new ProducerRequest("topic1", body, producer.ClientId, ProducerRequestType.send));
                    responses.Add(response);
                    Thread.Sleep(100);
                }
            });

            // Assert.AreEqual(responses.Count, 2);
            // Assert.AreEqual(responses[0].Topic, "topic1");
        }

        [TestMethod]
        public void Send_Message_to_Queue_with_Mulit_Thread_Diff_Producer_Same_Topic()
        {
            List<ProducerResponse> responses = new List<ProducerResponse>();
            Parallel.For(0, TestConfig.producerCount, (i) =>
            {
                i = Interlocked.Increment(ref i);
                //lock (lockobj)
                //{
                var producer = CreateProducer();
                producer.ConnectToServer();
                var body = Encoding.UTF8.GetBytes("Sender:" + i);
                var response = producer.SendRequest(new ProducerRequest("topic1", body, producer.ClientId, ProducerRequestType.send));
                responses.Add(response);
                Console.WriteLine(i);
                //      Thread.Sleep(50);
                // }
            });
        }

        [TestMethod]
        public void Send_Message_to_Queue_with_Single_Thread_different_Producer()
        {
            List<ProducerResponse> responses = new List<ProducerResponse>();


            for (int i = 0; i < TestConfig.producerCount; i++)
            {

                var producer = CreateProducer();
                producer.ConnectToServer();
                var body = Encoding.UTF8.GetBytes("Sender:" + i);
                var response = producer.SendRequest(new ProducerRequest("topic1", body, producer.ClientId, ProducerRequestType.send));
                responses.Add(response);
                Thread.Sleep(100);
            }

            //  Assert.AreEqual(responses.Count, TestConfig.Producer_Send_Count);
            //  Assert.IsTrue(responses.All(x => x.SendSuccess));
        }

        [TestMethod]
        public void Start_Producer_Whole_Action_With_Single_Producer()
        {
            List<ProducerResponse> responses = new List<ProducerResponse>();
            int sendCount = 0;
            bool isOver = false;

            var producer = CreateProducer();
            producer.BindTopic("topic1")
                 .SetBody(Encoding.UTF8.GetBytes(producer.ClientId.ToString()))
                    .BindSendCallBack((response) =>
                    {
                        Console.WriteLine(sendCount);
                        responses.Add(response);
                        sendCount++;
                        isOver = sendCount >= TestConfig.Producer_Send_Count;
                        if (isOver)
                        {
                            //     producer.Close();
                            // producer.Stop();
                            //    Assert.IsTrue(responses.All(x => x.SendSuccess));
                            return;
                        }
                    }).ConnectToServer();
            producer.Start();
            Console.ReadLine();
        }

        [TestMethod]
        public void Start_Producer_Whole_Action_Mulit_Producers()
        {

            List<ProducerResponse> responses = new List<ProducerResponse>();
            List<CamoranProducer> producers = new List<CamoranProducer>();
            int sendCount = 0;
            bool isOver = false;
            for (int i = 0; i < TestConfig.producerCount; i++)
            {
                var producer = CreateProducer();
                producers.Add(producer);
                producer.BindTopic("topic1")
              .SetBody(Encoding.UTF8.GetBytes(producer.ClientId.ToString()))
                 .BindSendCallBack((response) =>
                 {
                     lock (lockobj)
                     {
                         Console.WriteLine(sendCount);
                         responses.Add(response);
                         sendCount++;
                         isOver = sendCount >= TestConfig.Producer_Send_Count;
                         if (isOver)
                         {
                             producer.Close();
                             producer.Stop();
                             var count = responses.Count;
                             //Assert.IsTrue(responses.All(x => x.SendSuccess));
                            // Console.Read();
                             return;
                         }
                     }
                 }).ConnectToServer();
                producer.Start();

            }
            Console.ReadLine();
        }






        public CamoranProducer CreateProducer()
        {

            ClientConfig config = new ClientConfig { Address = _address, Port = _port };
            var producerId = Guid.NewGuid();
            var inner = new Client_byHelios<ProducerRequest, ProducerResponse>(producerId, config);
            var producer = new CamoranProducer(producerId, config, inner);
            return producer;
        }
    }
}

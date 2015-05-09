using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Camoran.Queue.Client.Consumer;
using System.Configuration;
using Camoran.Queue.Client;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace Camoran.Queue.UnitTest.Client.Consumer
{
    [TestClass]
    public class ConsumerTest
    {
        string topic = "topic1";
        private readonly string _address = TestConfig.BrokerAddress;// ConfigurationManager.AppSettings["BrokerAddress"];
        private readonly int _port = TestConfig.ConsumerListenerPort; //Convert.ToInt32(ConfigurationManager.AppSettings["BorkerPort"]);
        Guid[] guids = { 
                           Guid.Parse("425831d6-7f30-480c-9296-1ae21c3b3cca"),
                           Guid.Parse("9e6feb22-e07e-4f62-9163-7e30c06446c0"),
                           Guid.Parse("11e9eb8d-6bc5-42ac-b1a8-0f54d3e22152"),
                           Guid.Parse("36b15d0a-2128-490d-afb9-aefc5e16f203"),

                           Guid.Parse("fa3d9685-f9e6-4fca-87ef-e33f5c0596ee"),
                           Guid.Parse("d53628ae-4d8c-4792-86ac-53dcf97b4aff"),

                           Guid.Parse("ceb43f62-6111-48a3-9af1-044d7e9e24e4"),
                           Guid.Parse("f4c6f62b-8f27-4bd1-9b3c-1a9403ef3913"),

                           Guid.Parse("00ba636c-b156-4534-a1c0-54092731736c"),
                           Guid.Parse("8dc559e3-a00b-4a1f-a29f-57c159043e6c"),
                       };

        public Guid[] Guids
        {

            get { return this.guids; }
        }
        public ConsumerTest()
        {
        }

        [TestMethod]
        public void Send_Consume_Request_With_Single_Thread_Same_Consumer_Same_Topic()
        {
            var consumer = CreateConsumer(Guid.NewGuid());
            consumer.ConnectToServer();
            byte[] body = System.Text.Encoding.UTF8.GetBytes("Hello World");
            ConsumerRequest consumeRequest = new ConsumerRequest(topic, body, consumer.ClientId, ConsumerRequestType.consume);
            var response = consumer.SendRequest(consumeRequest);
            string helloWorld = Encoding.UTF8.GetString(response.Body);
            Assert.AreEqual(response.Topic, "topic1");
            Assert.AreEqual(helloWorld, "Hello World");
            Assert.AreEqual(response.CanConsume, false);

        }

        [TestMethod]
        public void Send_Consume_Request_With_Single_Thread_Diff_Consumer_Same_Topic()
        {
            for (int i = 0; i < guids.Length; i++)
            {
                var consumer = CreateConsumer(guids[i]);
                consumer.ConnectToServer();
                ConsumerRequest consumeRequest = new ConsumerRequest(topic, null, consumer.ClientId, ConsumerRequestType.consume);
                var response = consumer.SendRequest(consumeRequest);
            }
        }

        [TestMethod]
        public void Send_Consume_Callback_With_Single_Thread_Same_Consumer()
        {
            var consumer = CreateConsumer(Guid.NewGuid());
            consumer.ConnectToServer();
            ConsumerRequest callBackRequest = new ConsumerRequest(topic, null, consumer.ClientId, ConsumerRequestType.callback);
            ConsumerResponse response = consumer.SendRequest(callBackRequest);
            Assert.AreEqual(response.Topic, "topic1");
            Assert.AreEqual(response.ClientCurrentStatus, ClientStatus.wait);
        }

        [TestMethod]
        public void Send_Consume_Callback_with_Single_Thread_Diff_Consumers()
        {
            List<ConsumerResponse> responses = new List<ConsumerResponse>();
            for (int i = 0; i < guids.Length; i++)
            {
                var consumer = CreateConsumer(guids[i]);
                consumer.ConnectToServer();
                var callBackRequest = new ConsumerRequest(topic, null, consumer.ClientId, ConsumerRequestType.callback);
                var response = consumer.SendRequest(callBackRequest);
                responses.Add(response);
            }

            Assert.AreEqual(responses.Count, guids.Length);
            Assert.IsTrue(responses.Any(x => x.ClientCurrentStatus == ClientStatus.wait));
        }


        [TestMethod]
        public void Start_Consume_Whole_Action_with_Single_Thread_Same_Consumer_Test()
        {
            int consumerCount = 1;
            var body = System.Text.Encoding.UTF8.GetBytes("Hello World");

            bool canStop = false;
            var consumer = CreateConsumer(guids[0])
                .SubscribeTopic("topic1")
                .SetMessageBody(body)
                .RegisteConsumeAction((response) =>
            {
                var responseBody = System.Text.Encoding.UTF8.GetString(response.Body);
                Debug.WriteLine(responseBody);
                consumerCount++;
                canStop = consumerCount == TestConfig.Producer_Send_Count;
            });

            consumer.Start();
            while (true)
            {
                if (canStop)
                {
                    consumer.Stop();
                    break;
                }
            }

            Assert.AreEqual(consumerCount, TestConfig.Producer_Send_Count);
        }

        private object obj = new object();
        [TestMethod]
        public void Start_Consume_Whole_Action_with_Mulit_Thread_Diff_Consumer_Test()
        {
            bool canStop = false;
            int consumeCount = 0;
            List<CamoranConsumer> consumers = new List<CamoranConsumer>();
            Parallel.For(0, TestConfig.consumerCount, (i) =>
            {
                i = Interlocked.Increment(ref i);

                var body = System.Text.Encoding.UTF8.GetBytes("Hello World" + i);
                var consumer = CreateConsumer(guids[i--])
               .SubscribeTopic("topic1")
               .SetMessageBody(body)
               .RegisteConsumeAction((response) =>
               {
                   var responseBody = System.Text.Encoding.UTF8.GetString(response.Body);
                   var queueMessageBody = System.Text.Encoding.UTF8.GetString(response.QueueMeesageBody);
                   Console.WriteLine(responseBody
                       + "     " +
                       consumeCount++
                       + " QueueMessageId:" + response.QueueMessageId
                       + " QueueMessageBody:" + queueMessageBody
                       );
                   consumeCount++;
                   canStop = consumeCount == TestConfig.Producer_Send_Count;
               });
                consumers.Add(consumer);
                consumer.Start();


            });

            //while (true)
            //{
            //    if (canStop)
            //    {
            //        foreach (var consumer in consumers)
            //        {
            //             consumer.Stop();
            //        }
            //        break;
            //    }
            //}
            Console.ReadLine();
            Assert.AreEqual(consumeCount, TestConfig.Producer_Send_Count);
            //Assert.AreEqual();
        }

        public CamoranConsumer CreateConsumer(Guid id)
        {
            HostConfig config = new HostConfig { Address = _address, Port = _port };
            var producer = new CamoranConsumer(id, config);
            return producer;
        }




    }
}

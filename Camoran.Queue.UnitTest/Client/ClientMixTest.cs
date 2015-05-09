using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.UnitTest.Client.Consumer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.UnitTest.Client
{
    [TestClass]
    public class ClientMixTest
    {

        ProducerTest produerTestcase = new ProducerTest();
        ConsumerTest consumerTestcase = new ConsumerTest();
        string topic = "topic2";
        int sendCount = 0, consumeCount = 0;
        bool isSendOver = false, isConsumeOver = false;
        string sendString = "Hello World Producer";
        string receiveString = string.Empty;

        [TestMethod]
        public void Single_Sender_Send_Message_with_Single_Consmer()
        {

            byte[] sendBody = Encoding.UTF8.GetBytes(sendString);
            CamoranProducer producer = null;
            CamoranConsumer consumer = null;

            producer = produerTestcase
               .CreateProducer()
               .BindTopic(topic)
               .SetBody(sendBody)
               .BindSendCallBack((response) =>
               {
                   sendCount++;
                   Debug.WriteLine("sendCount:" + sendCount);
                   isSendOver = sendCount == TestConfig.Producer_Send_Count;
                   if (isSendOver)
                   {
                       producer.Stop();
                       producer.Close();
                   }
               });
            ;


            consumer = consumerTestcase.CreateConsumer(Guid.NewGuid())
                    .SubscribeTopic(topic)
                    .RegisteConsumeAction((response) =>
                    {
                        consumeCount++;
                        Debug.WriteLine("ConsumeCount:" + consumeCount);
                        isConsumeOver = consumeCount >= TestConfig.Producer_Send_Count;
                        if (isConsumeOver)
                        {
                            consumer.Stop();
                            consumer.Close();
                        }
                        receiveString = Encoding.UTF8.GetString(response.QueueMeesageBody);
                    });
            ;

            producer.Start();
            consumer.Start();

            //producer.Close();
            //producer.Stop();
            Assert.IsTrue(isConsumeOver);

            Assert.AreEqual(sendString, receiveString);
        }


        public void Multi_Sender_Mulit_Send_When_Consuming_Same_Consumer()
        {

        }
        [TestMethod]
        public void Single_Sender_Send_Message_With_Multi_Conumsers()
        {
            byte[] sendBody = Encoding.UTF8.GetBytes(sendString);
            CamoranProducer producer = this.CreateAndInitialProducer
                (
                  topic,
                  sendBody,
                  (response) =>
                  {
                      sendCount++;
                      Debug.WriteLine("sendCount:" + sendCount);
                      isConsumeOver = sendCount >= TestConfig.Producer_Send_Count;
                  }
                );
            producer.Start();
            while (!isConsumeOver) { }
            Parallel.For(0, TestConfig.consumerCount, (i) =>
            {
                i = Interlocked.Increment(ref i);
                var consumer = this.CreateAndInitialConsumer
                    (
                     consumerTestcase.Guids[i--],
                     topic,
                     null,
                     (response) =>
                     {
                         consumeCount++;
                         Debug.WriteLine("consumer id:" + response.SenderId + "consumeCount:" + consumeCount);
                     }
                    );

                consumer.Start();
            });
        }

        [TestMethod]
        public void Multi_Sender_Send_WhenMulit_Consumers()
        {

            this.produerTestcase.Send_Message_to_Queue_with_Single_Thread_different_Producer();

            Parallel.For(0, TestConfig.consumerCount, (i) =>
            {
                i = Interlocked.Increment(ref i);
                var consumer = this.CreateAndInitialConsumer
                    (
                     consumerTestcase.Guids[i--],
                     topic,
                     null,
                     (response) =>
                     {
                         consumeCount++;
                         Debug.WriteLine("consumer id:" + response.SenderId + "consumeCount:" + consumeCount);
                     }
                    );

                consumer.Start();
            });
        }


        public void Diff_Topic_between_Sender_Conumer()
        {
            string[] topics = { "topic1", "topic2", "topic3" };
        }

        public void Send_And_Consume_Capibility_Test()
        {

        }

        private CamoranProducer CreateAndInitialProducer(string topic, byte[] sendBody, Action<ProducerResponse> callback)
        {
            CamoranProducer producer = produerTestcase
                     .CreateProducer()
                     .BindTopic(topic)
                     .SetBody(sendBody)
                     .BindSendCallBack(callback);
            return producer;
        }


        private CamoranConsumer CreateAndInitialConsumer(Guid consumerId, string topic, byte[] sendBody, Action<ConsumerResponse> callback)
        {
            var consumer = consumerTestcase.CreateConsumer(consumerId)
               .SubscribeTopic(topic)
               .RegisteConsumeAction(callback);
            return consumer;
        }





    }
}

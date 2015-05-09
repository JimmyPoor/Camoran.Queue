using Camoran.Queue.Core.Queue;
using Camoran.Queue.Core.Message;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Camoran.Queue.UnitTest.Core
{
    [TestClass]
    public class MessageQueueTest
    {

        MessageQueue queue = new MessageQueue(Guid.NewGuid());

        [TestMethod]
        public void Single_Enqueue_When_Initial_Range_Test()
        {
            queue.SetAllowMessageSize(12000);
            queue.SetIntialMessageSize(12000);
            for (int i = 0; i < 1000; i++)
            {
                queue.Enqueue(new QueueMessage("demo_topic"));
            }

            Assert.IsTrue(queue.Size() == 1000);
        }

        [TestMethod]
        public void Single_Enqueue_When_LargeThan_AllowedRange_Test()
        {
            queue.SetIntialMessageSize(111000);
            queue.SetAllowMessageSize(112000);
            QueueMessage msg = new QueueMessage("demo_topic");
            for (int i = 0; i < 100000; i++)
            {
                queue.Enqueue(msg);
            }

            Assert.IsTrue(queue.Size() == 100000);
        }

        object lockobj = new object();
        [TestMethod]
        public void Muliple_Threads_Enqueue_Test()
        {
            queue.SetAllowMessageSize(100000);
            queue.SetIntialMessageSize(95000);
            Parallel.For(0, 100000, (j) =>
            {
                queue.Enqueue(new QueueMessage(j + ""));
            }
          );
            Assert.AreEqual(queue.Size(), 100000);
        }

        [TestMethod]
        public void Single_Threads_Dequeue_Test()
        {
            for (int i = 0; i < 100; i++)
            {
                queue.Enqueue(new QueueMessage("topic" + i));

                if (i % 2 == 0)
                {
                    queue.Dequeue();
                }
            }

            var messages = queue.Messages;
            Assert.AreEqual(messages.Count(), 52);
            //Assert.AreEqual(messages[1].Topic, "topic50");
        }


        [TestMethod]
        public void Muliple_Threads_Dequeue_Test()
        {
            queue.SetAllowMessageSize(10000);
            Parallel.For(0, 9000, (j) =>
            {
                queue.Enqueue(new QueueMessage(j + ""));
            }
          );
            int[] nums = Enumerable.Range(0, 8000).ToArray();
            Parallel.For(0, 8000, (j) =>
            {
                queue.Dequeue();
            }
            );

            var messages = queue.Messages;
            Assert.AreEqual(queue.Size(), 1000);
        }


        [TestMethod]
        public void Muliple_Threads_Mixed_Test()
        {
            int total = 0;
            Parallel.For(0, 2, (j) =>
            {
                total = Interlocked.Increment(ref total);
                queue.Dequeue();
                queue.Enqueue(new QueueMessage(total + ""));
                queue.Enqueue(new QueueMessage(total + ""));
                queue.Dequeue();
            });
            Assert.AreEqual(queue.Size(), 2);

        }
    }
}

using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using Camoran.Queue.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Camoran.Queue.Core.Store;
using System.Collections.Concurrent;

namespace Camoran.Queue.Broker.Message
{
    public class CamoranMessageManager : ICamoranMessageManager
    {
        protected ICamoranBrokerSession Session { get; private set; }
        public IConsumerMessageBuilder ConsumerMessageBuilder { get; protected set; }
        public IProducerMessageBuilder ProducerMessageBuilder { get; protected set; }
        public IQueueMessageStore MessageStore { get; protected set; }

        public ConcurrentDictionary<Guid, IList<QueueMessage>> PublishedMessages { get; set; }

        public CamoranMessageManager(ICamoranBrokerSession session)
        {
            this.Session = session;
            this.MessageStore = new MemoryMessageStore();
            ConsumerMessageBuilder = new ConsumerMessageBuilder();
            ProducerMessageBuilder = new ProducerMessageBuilder();
            PublishedMessages = new ConcurrentDictionary<Guid, IList<QueueMessage>>();
        }

        public bool RemovePublishedMessage(Guid consumerId, Guid queueMessageId)
        {
            bool queueMessageExists = false;
            IList<QueueMessage> publishedMessages = this.GetPublishMessagesByConsumerId(consumerId, out queueMessageExists);
            if (queueMessageExists)
            {
                var message = publishedMessages.FirstOrDefault(x => x.MessageId == queueMessageId);
                publishedMessages.Remove(message);
            }
            Console.WriteLine("publish message been removed");
            return false;
        }

        public void RemovePublishMessagesByConsumerId(Guid consumerId)
        {
            IList<QueueMessage> messages;

            if (Session.MessageManager.PublishedMessages.ContainsKey(consumerId))
            {
                Session.MessageManager.PublishedMessages.TryRemove(consumerId, out messages);
            }
        }

        public IList<QueueMessage> GetPublishMessagesByConsumerId(Guid consumerId, out bool messageExists)
        {
            IList<QueueMessage> queueMessags = null;
            messageExists = Session.MessageManager.PublishedMessages.TryGetValue(consumerId, out queueMessags);
            return queueMessags;
        }

        public bool TrySendMessage(string topic, Guid senderId, QueueMessage message)
        {
            IList<CamoranProducer> producers = null;
            IList<MessageQueue> queues = null;
            var queuesIsExists = Session.QueueService.TopicQueues.TryGetValue(topic, out queues);
            if (!queuesIsExists) return false;
            Session.ProducerManger.MappingListBetweenTopicAndClients.TryGetValue(topic, out producers);
            int producerIndex = producers.GetIndexInList(
                 (producer, id) => producer.ClientId == id
                , senderId);// find index in producers
            int queueIndx = this.Session.QueueService.FindQueueIndex(queues.Count, producerIndex);
            var queue = queues[queueIndx];
            queue.Enqueue(message);
            return true;
        }

        public void PublishMessage(string topic, QueueMessage message)
        {
            var consumer = this.Session.ConsumerManager.FindConsumerByQueueId(topic, message.QueueId);
            if (consumer != null)
            {
                consumer.SetClientStatus(Camoran.Queue.Client.ClientStatus.ready);
                consumer.CurrentQueueMessage = message;

                Session.MessageManager.PublishedMessages.AddOrUpdate(
                    consumer.ClientId,
                    new List<QueueMessage> { message },
                    (key, old) =>
                    {
                        old.Add(message);
                        return old;
                    });
            }
        }
    }
}

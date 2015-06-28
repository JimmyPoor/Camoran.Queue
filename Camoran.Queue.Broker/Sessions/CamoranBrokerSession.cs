using Camoran.Queue.Broker.Client;
using Camoran.Queue.Broker.Listeners;
using Camoran.Queue.Broker.Message;
using Camoran.Queue.Broker.Queue;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using Camoran.Queue.Core.Store;
using Camoran.Queue.Util.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Camoran.Queue.Broker.Sessions
{

    public class CamoranBrokerSession : ICamoranBrokerSession
    {

        public ConcurrentDictionary<string, IList<CamoranConsumer>> MappingListBetweenTopicAndConsumers { get; set; }
        public ConcurrentDictionary<string, IList<CamoranProducer>> MappingListBetweenTopicAndProducers { get; set; }
        public ConcurrentDictionary<string, IList<MessageQueue>> TopicQueues { get; set; }
        public ConcurrentDictionary<Guid, IList<QueueMessage>> PublishedMessages { get; set; }
        public Guid SessionId { get; private set; }

        public ICamoranClientManager ClientManager { get; private set; }

        public ICamoranClientStrategy ClientStrategy { get; private set; }

        public ICamoranQueueService QueueService { get; private set; }

        public ICamoranMessageManager MessageManager { get; private set; }


        public CamoranBrokerSession()
        {
            this.ConsumerListener = new CamoranConsumerListener();
            this.ProducerListener = new CamoranProducerListener();
            MappingListBetweenTopicAndConsumers = new ConcurrentDictionary<string, IList<CamoranConsumer>>();
            MappingListBetweenTopicAndProducers = new ConcurrentDictionary<string, IList<CamoranProducer>>();
            TopicQueues = new ConcurrentDictionary<string, IList<MessageQueue>>();
            PublishedMessages = new ConcurrentDictionary<Guid, IList<QueueMessage>>();
            SessionId = Guid.NewGuid();
            ClientManager = new CamoranClientManager(this);
            ClientStrategy = new CamoranClientStrategy(this);
            QueueService = new CamoranQueueService(this);
            MessageManager = new CamoranMessageManager(this);
        }

        public IClientListener ConsumerListener { get; private set; }

        public IClientListener ProducerListener { get; private set; }

        public CamoranConsumer CreateOrGetConsumer(Guid consumerId)
        {
            var consumer = MappingListBetweenTopicAndConsumers.Values
                .SelectMany(x => x)
                .SingleOrDefault(y => y.ClientId == consumerId);
            consumer = consumer ?? new CamoranConsumer(consumerId, null);
            return consumer;
        }

        public CamoranProducer CreateOrGetProducer(Guid producerId)
        {
            var producer = MappingListBetweenTopicAndProducers.Values.
                SelectMany(x => x)
               .FirstOrDefault(y => y.ClientId == producerId);
            return producer ?? new CamoranProducer(producerId, null);
        }

        public void SubscribeConsumer(string topic, CamoranConsumer consumer)
        {
            if (consumer != null)
            {
                consumer.SubscribeTopic(topic);
                var topicConsumerList = MappingListBetweenTopicAndConsumers.GetOrAdd(topic, new List<CamoranConsumer>());
                if (topicConsumerList.Any(x => x.ClientId == consumer.ClientId))
                {
                    return;
                }
                else
                {
                    topicConsumerList.Add(consumer);
                }
            }
        }

        public void SubscribeProducer(string topic, CamoranProducer producer)
        {
            if (producer != null)
            {
                producer.BindTopic(topic);
                var topicConsumerList = MappingListBetweenTopicAndProducers.GetOrAdd(topic, new List<CamoranProducer>());
                if (topicConsumerList.Any(x => x.ClientId == producer.ClientId))
                {
                    return;
                }
                else
                {
                    topicConsumerList.Add(producer);
                }
            }
        }



     
    }
}
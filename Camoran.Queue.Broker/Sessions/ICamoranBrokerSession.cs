using Camoran.Queue.Broker.Client;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Camoran.Queue.Broker.Sessions
{
    public interface ICamoranBrokerSession : IBrokerSession<CamoranConsumer, CamoranProducer>
    {
        ConcurrentDictionary<string, IList<CamoranConsumer>> MappingListBetweenTopicAndConsumers { get; set; }
        ConcurrentDictionary<string, IList<CamoranProducer>> MappingListBetweenTopicAndProducers { get; set; }
        ConcurrentDictionary<string, IList<MessageQueue>> TopicQueues { get; set; }
        ConcurrentDictionary<Guid, IList<QueueMessage>> PublishedMessages { get; set; }
        ICamoranClientManager ClientManager { get; }
        ICamoranQueueService QueueService { get; }
        bool RemovePublishedMessage(Guid consumerId, Guid queueMessageId);

        void RemovePublishMessagesByConsumers(IEnumerable<CamoranConsumer> removedConsumers);

        void RemovePublishMessagesByConsumer(CamoranConsumer consumer);

        IList<QueueMessage> GetPublishMessagesByConsumerId(Guid consumerId, out bool messageExists);

        IList<QueueMessage> GetPublishMessagesByConsumers(IEnumerable<CamoranConsumer> consumers);
    }
}

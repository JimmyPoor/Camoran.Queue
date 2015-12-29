using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Message
{
    public interface ICamoranMessageManager
    {
        IList<QueueMessage> GetPublishMessagesByConsumerId(Guid consumerId, out bool messageExists);
        IQueueMessageStore MessageStore { get; }
        IConsumerMessageBuilder ConsumerMessageBuilder { get;}
         IProducerMessageBuilder ProducerMessageBuilder { get;}
        ConcurrentDictionary<Guid, IList<QueueMessage>> PublishedMessages { get; set; }

        bool RemovePublishedMessage(Guid consumerId, Guid queueMessageId);
        void RemovePublishMessagesByConsumerId(Guid consumerId);
        bool TrySendMessage(string topic, Guid senderId, QueueMessage message);
        void PublishMessage(string topic, QueueMessage message);

    }
}

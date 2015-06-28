using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Message
{
    public interface ICamoranMessageManager
    {
        bool RemovePublishedMessage(Guid consumerId, Guid queueMessageId);

        void RemovePublishMessagesByConsumerId(Guid consumerId);

        IList<QueueMessage> GetPublishMessagesByConsumerId(Guid consumerId, out bool messageExists);

        bool TrySendMessage(string topic, Guid senderId, QueueMessage message);
        void PublishMessage(string topic, QueueMessage message);
    }
}

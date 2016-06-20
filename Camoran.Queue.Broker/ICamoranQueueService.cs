using Camoran.Queue.Core;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker
{
    public interface ICamoranQueueService : IQueueService<MessageQueue>
    {

        ConcurrentDictionary<string, IList<MessageQueue>> TopicQueues { get; set; }
        MessageQueueProcessor QueueProcessor { get;}
        IList<MessageQueue> CreateTopicQueuesIfNotExists(string topic, int createQueueCount);
        int FindQueueIndex(int queueCount, int producerIndex);
        void StartQueues(IEnumerable<MessageQueue> topicQueues, Func<MessageQueue, bool> canInvoke, Action<QueueMessage> whenInvoke);
    }

}

using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Camoran.Queue.Broker.Queue
{
    public class CamoranQueueService : ICamoranQueueService
    {
        private ICamoranBrokerSession _session;

        public MessageQueueProcessor QueueProcessor { get; protected set; }
        public CamoranQueueService(ICamoranBrokerSession session)
        {
            this._session = session;
            TopicQueues = new ConcurrentDictionary<string, IList<MessageQueue>>();
            QueueProcessor = new CamoranMQProcessor();
        }

        public ConcurrentDictionary<string, IList<MessageQueue>> TopicQueues { get; set; }

        public IList<Core.Queue.MessageQueue> CreateTopicQueuesIfNotExists(string topic, int createQueueCount)
        {
            var queuesWithTopic = TopicQueues.GetOrAdd(topic, (key) =>
            {
                List<MessageQueue> queues = new List<MessageQueue>();
                for (int i = 0; i < createQueueCount; i++)
                {
                    queues.Add(new MessageQueue
                        (
                        Guid.NewGuid()
                        ));
                }
                return queues;
            });
            return queuesWithTopic;
        }

        public int FindQueueIndex(int queueCount, int producerIndex)
        {
            return producerIndex.GetHashCode()
              % queueCount;
        }

        public virtual void StartQueues(IEnumerable<MessageQueue> topicQueues, Func<MessageQueue, bool> canInvoke, Action<QueueMessage> whenInvoke)
        {
            if (canInvoke == null) return;
            if (whenInvoke == null) return;
            if (topicQueues == null || topicQueues.All(x => x.QueueStatus == QueueStatus.wroking)) return;
            foreach (var queue in topicQueues)
            {
                if (queue.QueueStatus == QueueStatus.stop)
                {
                    queue.SetQueueStatus(QueueStatus.wroking);
                    QueueProcessor.ProcessQueueAsync(
                      queue,
                      () =>
                      {
                          return canInvoke(queue);
                      },
                      (queueMessage) =>
                      {
                          whenInvoke(queueMessage);
                      }
               );
                }

            }
        }
    }
}
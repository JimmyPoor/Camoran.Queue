using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Generic;

namespace Camoran.Queue.Broker.Queue
{
    public class CamoranQueueService : ICamoranQueueService
    {
        private ICamoranBrokerSession _session;
        public CamoranQueueService(ICamoranBrokerSession session)
        {
            this._session = session;
        }
        public IList<Core.Queue.MessageQueue> CreateTopicQueuesIfNotExists(string topic, int createQueueCount)
        {
            var queuesWithTopic = _session.TopicQueues.GetOrAdd(topic, (key) =>
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

    }
}

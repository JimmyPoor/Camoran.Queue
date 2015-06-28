using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Util.Extensions;
using Camoran.Queue.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Client
{
    public class CamoranClientManager:ICamoranClientManager
    {
        private ICamoranBrokerSession _session;
        public CamoranClientManager(ICamoranBrokerSession session)
        {
            this._session = session;
        }

        public void RemoveConsumer(CamoranConsumer consumer)
        {
            IList<CamoranConsumer> consumers = null;
            bool exists = _session.MappingListBetweenTopicAndConsumers.TryGetValue(consumer.CurrentTopic, out consumers);
            if (exists && consumers != null)
            {
                consumers.Remove(consumer);
            }
        }
        public void RemoveConsumers(IEnumerable<CamoranConsumer> consumers)
        {
            var consumerList = consumers.ToList();
            for (int i = 0; i < consumerList.Count; i++)
            {
                RemoveConsumer(consumerList[i]);
            }
        }

        public void RemoveProducer(CamoranProducer producer)
        {
            IList<CamoranProducer> producers = null;
            bool exists = _session.MappingListBetweenTopicAndProducers.TryGetValue(producer.CurrentTopic, out producers);
            if (exists && producers != null)
            {
                producers.Remove(producer);
            }
        }

        public void RemoveProducers(IEnumerable<CamoranProducer> producers)
        {
            var producerList = producers.ToList();
            for (int i = 0; i < producerList.Count; i++)
            {
                RemoveProducer(producerList[i]);
            }
        }


        public IEnumerable<CamoranConsumer> FindTimeoutConsumers(int timeoutSeconds)
        {
            var timeoutConcumers = this.FindTimeoutClients(timeoutSeconds, _session.MappingListBetweenTopicAndConsumers);
            return timeoutConcumers;
        }

        public IEnumerable<CamoranProducer> FindTimeoutProducers(int timeoutSeconds)
        {
            var timeoutProducers = this.FindTimeoutClients(timeoutSeconds, _session.MappingListBetweenTopicAndProducers);
            return timeoutProducers;
        }

        private IEnumerable<T> FindTimeoutClients<T>(int timeoutSeconds, ConcurrentDictionary<string, IList<T>> source) where T : IClient
        {
            var timeoutClients = source
                .Values
                .SelectMany(x => x)
                .Where(x => x.IsTimeout(timeoutSeconds));

            return timeoutClients;
        }


        public CamoranConsumer FindConsumer(string topic, Guid fromQueueId)
        {
            IList<CamoranConsumer> consumers = null;
            IList<Camoran.Queue.Core.Queue.MessageQueue> queues = null;
            bool hasConsumers = _session.MappingListBetweenTopicAndConsumers.TryGetValue(topic, out consumers);
            bool hasQueues = _session.TopicQueues.TryGetValue(topic, out queues);

            if (!hasConsumers || consumers.Count <= 0) return null;
            if (!hasQueues) throw new ApplicationException("queues should exist!");

            var queueIndex = queues.GetIndexInList(
                  (queue, id) => queue.QueueId == id,
                  fromQueueId);
            var consumerIndex = this.FindConsumerIndex(queues.Count, consumers.Count, queueIndex);
            var consumer = consumers[consumerIndex];

            return consumer;
        }

        public virtual int FindConsumerIndex(int queueCount, int consumerCount, int queueIndex)
        {
            if (consumerCount == 0) throw new ArgumentOutOfRangeException("consumer count should be more than zero");
            double consumerIndex = 0;
            double gap = Math.Ceiling((double)(queueCount / consumerCount));
            gap = gap == 0 ? 1 : gap;
            double queueCurrentRnageStartIndex = 0;
            double queueNextRangeStartIndex = 0;
            for (int i = 0; i < consumerCount; i++)
            {
                queueCurrentRnageStartIndex = (i * gap) % queueCount;
                queueNextRangeStartIndex = ((i + 1) * gap) % queueCount;

                if (queueIndex >= queueNextRangeStartIndex
                    && i < consumerCount - 1
                    && queueIndex <= queueCount - 1) { continue; }
                else
                {
                    consumerIndex = i;
                    break;
                }
            }

            return (int)consumerIndex;
        }
    }
}

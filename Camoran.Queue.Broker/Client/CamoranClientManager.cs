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

    public class CamoranConsumerManager : ICamoranConsumerManager
    {
        ICamoranBrokerSession _session;
        public CamoranConsumerManager(ICamoranBrokerSession session)
        {
            this._session = session;
            MappingListBetweenTopicAndClients = new ConcurrentDictionary<string, IList<CamoranConsumer>>();
        }
        public ConcurrentDictionary<string, IList<CamoranConsumer>> MappingListBetweenTopicAndClients { get; set; }

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

        public CamoranConsumer FindConsumerByQueueId(string topic, Guid fromQueueId)
        {
            IList<CamoranConsumer> consumers = null;
            IList<Camoran.Queue.Core.Queue.MessageQueue> queues = null;
            bool hasConsumers = MappingListBetweenTopicAndClients.TryGetValue(topic, out consumers);
            bool hasQueues = _session.QueueService.TopicQueues.TryGetValue(topic, out queues);

            if (!hasConsumers || consumers.Count <= 0) return null;
            if (!hasQueues) throw new ApplicationException("queues should exist!");

            var queueIndex = queues.GetIndexInList(
                  (queue, id) => queue.QueueId == id,
                  fromQueueId);
            var consumerIndex = this.FindConsumerIndex(queues.Count, consumers.Count, queueIndex);
            var consumer = consumers[consumerIndex];

            return consumer;
        }

        public IEnumerable<CamoranConsumer> FindTimeoutClient(int timeoutSeconds)
        {
            var timeoutClients = MappingListBetweenTopicAndClients
            .Values
            .SelectMany(x => x)
            .Where(x => x.IsTimeout(timeoutSeconds));

            return timeoutClients;
        }

        public void RemoveClients(IEnumerable<CamoranConsumer> consumers)
        {
            var consumerList = consumers.ToList();
            for (int i = 0; i < consumerList.Count; i++)
            {
                RemoveClinet(consumerList[i]);
            }
        }

        public void RemoveClinet(CamoranConsumer consumer)
        {
            IList<CamoranConsumer> consumers = null;
            bool exists = MappingListBetweenTopicAndClients.TryGetValue(consumer.CurrentTopic, out consumers);
            if (exists && consumers != null)
            {
                consumers.Remove(consumer);
            }
        }

        public CamoranConsumer GetClient(Guid clientId)
        {
            var producer = MappingListBetweenTopicAndClients.Values.
           SelectMany(x => x)
          .FirstOrDefault(y => y.ClientId == clientId);
            return producer;
        }

        public void AddClient(string topic, CamoranConsumer consumer)
        {
            var topicConsumerList = this.MappingListBetweenTopicAndClients.GetOrAdd(topic, new List<CamoranConsumer>());
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


    public class CamoranProducerManager : ICamoranProducerManager
    {
        ICamoranBrokerSession _session;
        public CamoranProducerManager(ICamoranBrokerSession session)
        {
            MappingListBetweenTopicAndClients = new ConcurrentDictionary<string, IList<CamoranProducer>>();
            this._session = session;
        }
        public ConcurrentDictionary<string, IList<CamoranProducer>> MappingListBetweenTopicAndClients { get; set; }

        public CamoranProducer GetClient(Guid clientId)
        {
            var producer = MappingListBetweenTopicAndClients.Values.
               SelectMany(x => x)
              .FirstOrDefault(y => y.ClientId == clientId);
            return producer;
        }

        public IEnumerable<CamoranProducer> FindTimeoutClient(int timeoutSeconds)
        {
            var timeoutClients = MappingListBetweenTopicAndClients
          .Values
          .SelectMany(x => x)
          .Where(x => x.IsTimeout(timeoutSeconds));

            return timeoutClients;
        }

        public void RemoveClients(IEnumerable<CamoranProducer> producers)
        {
            var producerList = producers.ToList();
            for (int i = 0; i < producerList.Count; i++)
            {
                RemoveClinet(producerList[i]);
            }
        }

        public void RemoveClinet(CamoranProducer producer)
        {
            IList<CamoranProducer> producers = null;
            if (producer == null || producer.CurrentTopic == null) return;
            bool exists = MappingListBetweenTopicAndClients.TryGetValue(producer.CurrentTopic, out producers);
            if (exists && producers != null)
            {
                producers.Remove(producer);
            }
        }

        public void AddClient(string topic, CamoranProducer producer)
        {
            var topicConsumerList = this.MappingListBetweenTopicAndClients.GetOrAdd(topic, new List<CamoranProducer>());
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
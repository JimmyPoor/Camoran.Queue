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

    public class CamoranClientManger<Client> : ICamoranClientManager<Client> where Client : IClient
    {
        public ConcurrentDictionary<string, IList<Client>> MappingListBetweenTopicAndClients { get; set; }
        protected ICamoranBrokerSession Sesson;
        public CamoranClientManger(ICamoranBrokerSession session)
        {
            this.Sesson = session;
            MappingListBetweenTopicAndClients = new ConcurrentDictionary<string, IList<Client>>();
        }

        public virtual void AddClient(string topic, Client client)
        {
            var topicClientList = this.MappingListBetweenTopicAndClients.GetOrAdd(topic, new List<Client>());
            if (topicClientList.Any(x => x.ClientId == client.ClientId))
            {
                return;
            }
            else
            {
                topicClientList.Add(client);
            }
        }

        public virtual IEnumerable<Client> FindTimeoutClient(int timeoutSeconds)
        {
            var timeoutClients = MappingListBetweenTopicAndClients
          .Values
          .SelectMany(x => x)
          .Where(x => x.IsTimeout(timeoutSeconds));
            return timeoutClients;
        }

        public virtual Client GetClient(Guid clientId)
        {
            var client = MappingListBetweenTopicAndClients.Values.
            SelectMany(x => x)
           .FirstOrDefault(y => y.ClientId == clientId);
            return client;
        }

        public virtual void RemoveClients(IEnumerable<Client> clients)
        {
            var clientList = clients.ToList();
            for (int i = 0; i < clientList.Count; i++)
            {
                RemoveClinet(clientList[i]);
            }
        }

        public virtual void RemoveClinet(Client client)
        {
            IList<Client> clients = null;
            bool exists = MappingListBetweenTopicAndClients.TryGetValue(client.CurrentTopic, out clients);
            if (exists && clients != null)
            {
                clients.Remove(client);
            }
        }
    }

    public class CamoranConsumerManager : CamoranClientManger<CamoranConsumer>, ICamoranConsumerManager
    {
        public CamoranConsumerManager(ICamoranBrokerSession session):base(session)
        {
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

        public CamoranConsumer FindConsumerByQueueId(string topic, Guid fromQueueId)
        {
            IList<CamoranConsumer> consumers = null;
            IList<Camoran.Queue.Core.Queue.MessageQueue> queues = null;
            bool hasConsumers = MappingListBetweenTopicAndClients.TryGetValue(topic, out consumers);
            bool hasQueues = Sesson.QueueService.TopicQueues.TryGetValue(topic, out queues);

            if (!hasConsumers || consumers.Count <= 0) return null;
            if (!hasQueues) throw new ApplicationException("queues should exist!");

            var queueIndex = queues.GetIndexInList(
                  (queue, id) => queue.QueueId == id,
                  fromQueueId);
            var consumerIndex = this.FindConsumerIndex(queues.Count, consumers.Count, queueIndex);
            var consumer = consumers[consumerIndex];

            return consumer;
        }
    }


    public class CamoranProducerManager : CamoranClientManger<CamoranProducer>,ICamoranProducerManager
    {
        public CamoranProducerManager(ICamoranBrokerSession session):base(session)
        {
        }
    }
}
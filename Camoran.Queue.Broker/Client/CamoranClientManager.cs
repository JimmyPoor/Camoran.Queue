using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
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
    }
}

using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Client
{

    public interface ICamoranClientManager
    {
        void RemoveConsumer(CamoranConsumer consumer);
        void RemoveConsumers(IEnumerable<CamoranConsumer> consumers);

        void RemoveProducer(CamoranProducer producer);

        void RemoveProducers(IEnumerable<CamoranProducer> producers);

        IEnumerable<CamoranConsumer> FindTimeoutConsumers(int timeoutSeconds);

        IEnumerable<CamoranProducer> FindTimeoutProducers(int timeoutSeconds);

        CamoranConsumer FindConsumer(string topic, Guid fromQueueId);

        int FindConsumerIndex(int queueCount, int consumerCount, int queueIndex);
    }
}

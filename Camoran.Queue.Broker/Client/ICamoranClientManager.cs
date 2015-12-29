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

    public interface ICamoranClientManager<Client> where Client:IClient
    {
        ConcurrentDictionary<string, IList<Client>> MappingListBetweenTopicAndClients { get; set; }
        Client GetClient(Guid clientId);
        void AddClient(string topic,Client client);
        void RemoveClinet(Client client);
        void RemoveClients(IEnumerable<Client> consumers);
        IEnumerable<Client> FindTimeoutClient(int timeoutSeconds);
    }

    public interface ICamoranConsumerManager: ICamoranClientManager<CamoranConsumer>
    {
        int FindConsumerIndex(int queueCount, int consumerCount, int queueIndex);
        CamoranConsumer FindConsumerByQueueId(string topic, Guid fromQueueId);
    }

    public interface ICamoranProducerManager: ICamoranClientManager<CamoranProducer>
    {

    }
}

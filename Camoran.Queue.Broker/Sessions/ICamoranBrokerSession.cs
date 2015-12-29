using Camoran.Queue.Broker.Client;
using Camoran.Queue.Broker.Message;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Camoran.Queue.Broker.Sessions
{
    public interface ICamoranBrokerSession : IBrokerSession<CamoranConsumer, CamoranProducer>
    {
        ICamoranConsumerManager ConsumerManager { get; }
        ICamoranProducerManager ProducerManger { get; }
        ICamoranClientBehavior ClientBehavior { get; }
        ICamoranQueueService QueueService { get; }
        ICamoranMessageManager MessageManager { get; }

        ICamoranBrokerSession BindListener(HostConfig config);

    }
}

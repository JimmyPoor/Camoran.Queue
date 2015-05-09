using Camoran.Queue.Client;
using Camoran.Queue.Core;
using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker
{

    public interface ISession
    {
        Guid SessionId { get; }
    }
    public interface IBrokerSession<Consumer, Producer> : ISession
    {
        Consumer CreateOrGetConsumer(Guid consumerId);
        Producer CreateOrGetProducer(Guid producerId);

  
        void SubscribeConsumer(string topic, Consumer consumer);
        void SubscribeProducer(string topic, Producer producer);

        IClientListener ConsumerListener { get; }
        IClientListener ProducerListener { get; }

    }
}

using Camoran.Queue.Broker.Client;
using Camoran.Queue.Broker.Listeners;
using Camoran.Queue.Broker.Message;
using Camoran.Queue.Broker.Queue;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using Camoran.Queue.Core.Store;
using Camoran.Queue.Util.Extensions;
using Camoran.Queue.Util.Serialize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Camoran.Queue.Broker.Sessions
{

    public class CamoranBrokerSession : ICamoranBrokerSession
    {
        //public ConcurrentDictionary<Guid, IList<QueueMessage>> PublishedMessages { get; set; }
        public Guid SessionId { get; private set; }
        public ICamoranConsumerManager ConsumerManager { get; private set; }
        public ICamoranProducerManager ProducerManger { get; private set; }
        public ICamoranClientBehavior ClientBehavior { get; private set; }

        public ICamoranQueueService QueueService { get; private set; }

        public ICamoranMessageManager MessageManager { get; private set; }

        public CamoranBrokerSession()
        {
            SessionId = Guid.NewGuid();
            ConsumerManager = new CamoranConsumerManager(this);
            ProducerManger = new CamoranProducerManager(this);
            ClientBehavior = new CamoranClientBehavior(this);
            QueueService = new CamoranQueueService(this);
            MessageManager = new CamoranMessageManager(this);
        }

        public IClientListener ConsumerListener { get; private set; }

        public IClientListener ProducerListener { get; private set; }

        public CamoranConsumer CreateOrGetConsumer(Guid consumerId)
        {
            var consumer = this.ConsumerManager.GetClient(consumerId);
            consumer = consumer ?? 
                new CamoranConsumer(
                consumerId,
                null,
                new Client_byHelios<ConsumerRequest,ConsumerResponse>(consumerId,null)
                );
            return consumer;
        }

        public CamoranProducer CreateOrGetProducer(Guid producerId)
        {
            var producer = this.ProducerManger.GetClient(producerId);
            return producer ?? new CamoranProducer(
             producerId,
             null,
             new Client_byHelios<ProducerRequest, ProducerResponse>(producerId, null)
             );
        }

        public void SubscribeConsumer(string topic, CamoranConsumer consumer)
        {
            consumer.SubscribeTopic(topic);
            this.ConsumerManager.AddClient(topic, consumer);
        }

        public void SubscribeProducer(string topic, CamoranProducer producer)
        {
            producer.BindTopic(topic);
            this.ProducerManger.AddClient(topic, producer);
        }

    
        public ICamoranBrokerSession BindListener(HostConfig config)
        {
            this.ConsumerListener = new CamoranClientListener_ByHelios<ConsumerRequest, ConsumerResponse>(
                config.ConsumerAddress, 
                config.ConsumerPort, 
                new ProtoBufSerializeProcessor(), 
                config.ServerWithAnyIPAddress); // need optimize later
            this.ProducerListener = new CamoranClientListener_ByHelios<ProducerRequest, ProducerResponse>(
                config.ProduceAddress, 
                config.ProducePort, 
                new ProtoBufSerializeProcessor(), 
                config.ServerWithAnyIPAddress);// need optimize later
            return this;

        }

  
    }
}
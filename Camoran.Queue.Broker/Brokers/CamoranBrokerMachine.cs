using Camoran.Queue.Broker.Client;
using Camoran.Queue.Broker.Listeners;
using Camoran.Queue.Broker.Queue;
using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using Camoran.Queue.Core.Store;
using Camoran.Queue.Util;
using Camoran.Queue.Util.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Brokers
{
    public class CamoranBrokerMachine
    {
        public IQueueMessageStore MessageStore { get; private set; }
        public ICamoranBrokerSession Session { get; private set; }
        public MessageQueueProcessor QueueProcessor { get; private set; }
        public IConsumerMessageBuilder ConsumerMessageBuilder { get; private set; }
        public IProducerMessageBuilder ProducerMessageBuilder { get; private set; }

        protected readonly int defaultQueueCountWithEeachTopic = 5;

        protected readonly int consumerTimeout = 20*1000;

        private CamoranConsumerListener _consumerListener;
        private CamoranProducerListener _producerListener;
        private System.Timers.Timer _startQueueScedule = new System.Timers.Timer();
        private System.Timers.Timer _removedConsumersScedule = new System.Timers.Timer();
        private Thread _consumerListenerThread = null;
        private Thread _producerListenerThread = null;
        private static object lockObj = new object();

        public CamoranBrokerMachine()
        {
            _startQueueScedule.SetSceduleWork(1000, (o, e) => StartQueues());
            _removedConsumersScedule.SetSceduleWork(consumerTimeout, (o, e) =>
                this.Session.ClientStrategy.ConsumerTimeout(consumerTimeout));
            this.ConsumerMessageBuilder = new ConsumerMessageBuilder();
            this.ProducerMessageBuilder = new ProducerMessageBuilder();
            this.MessageStore = new MemoryMessageStore();
        }


        public void Start()
        {
            try
            {
                _startQueueScedule.Start();
                _removedConsumersScedule.Start();
                StartClientListener(out _consumerListenerThread, _consumerListener);
                StartClientListener(out _producerListenerThread, _producerListener);
                while (true)
                {
                    if (!_consumerListenerThread.IsAlive || !_producerListenerThread.IsAlive)
                    {
                        break;
                    }
                }
            }
            catch
            {

            }

        }

        public void Stop()
        {
            _startQueueScedule.Close();
            _startQueueScedule.Dispose();
            _removedConsumersScedule.Close();
            _removedConsumersScedule.Dispose();
            _consumerListener.StopListen();
            _producerListener.StopListen();
            _consumerListenerThread.Abort();
            _producerListenerThread.Abort();
        }

        public CamoranBrokerMachine RegistProcessor(MessageQueueProcessor processor)
        {
            this.QueueProcessor = processor;
            return this;
        }

        public CamoranBrokerMachine RegistBrokerSession(ICamoranBrokerSession session)
        {
            if (session == null) throw new ArgumentNullException("session object can't be null or empty");
            this.Session = session;
            return this;
        }

        public CamoranBrokerMachine InitialClientListener()
        {
            if (Session == null) throw new NullReferenceException("please regist broker session first");
            _consumerListener = Session.ConsumerListener as CamoranConsumerListener;
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.consume.ToString(), ConsumerConsumeAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.callback.ToString(), ConsumerConsumeCallBackAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.disconnect.ToString(), ConsumerDisconnectAction());

            _producerListener = Session.ProducerListener as CamoranProducerListener;
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.send.ToString(), ProducerSendAction());
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.disconnect.ToString(), ProducerDisconnectAction());
            return this;
        }

        protected void StartQueues()
        {
            lock (lockObj)
            {
                var queues = Session.TopicQueues.Select(kv => kv);
                foreach (var kv in queues)
                {
                    this.StartQueues(kv.Key, kv.Value);
                }
            }
        }
        protected void StartQueues(string topic, IEnumerable<MessageQueue> topicQueues)
        {
            if (topicQueues.All(x => x.QueueStatus == QueueStatus.wroking)) return;
            foreach (var queue in topicQueues)
            {
                if (queue.QueueStatus == QueueStatus.stop)
                {
                    Thread.Sleep(10);
                    queue.SetQueueStatus(QueueStatus.wroking);

                    QueueProcessor.ProcessQueueAsync(
                      queue,
                      () =>
                      {
                          var consumer = this.Session.ClientManager.FindConsumer(topic, queue.QueueId);
                          return consumer == null ? false : consumer.Status == ClientStatus.wait;
                      },
                      (queueMessage) =>
                      {
                          this.Session.MessageManager.PublishMessage(topic, queueMessage);
                      }
               );
                }

            }
        }

        protected Func<ConsumerRequest, ConsumerResponse> ConsumerConsumeAction()
        {
            Func<ConsumerRequest, ConsumerResponse> consumeAction = (request) =>
            {

                ConsumerResponse response = null;
                bool canConsume = false;
                var consumer = Session.CreateOrGetConsumer(request.SenderId);
                Session.SubscribeConsumer(request.Topic, consumer);
                this.Session
                    .QueueService
                    .CreateTopicQueuesIfNotExists(request.Topic, defaultQueueCountWithEeachTopic);

                canConsume = consumer.Status == ClientStatus.ready;
                if (canConsume)
                {
                    consumer.StartWorkingDate = DateTime.Now;
                    consumer.SetClientStatus(ClientStatus.working);
                }
                response = ConsumerMessageBuilder.BuildConsumerResponseMessage(
                    topic: request.Topic,
                    body: request.Body,
                    senderId: request.SenderId
                    );
                response.CanConsume = canConsume;
                var consumingMessage = consumer.CurrentQueueMessage;
                if (consumingMessage != null)
                {
                    response.QueueMessageId = consumingMessage.MessageId;
                    response.QueueMeesageBody = consumingMessage.Body;
                }
                return response;
            };

            return consumeAction;
        }

        protected Func<ConsumerRequest, ConsumerResponse> ConsumerConsumeCallBackAction()
        {
            Func<ConsumerRequest, ConsumerResponse> callbackAction = (request) =>
            {
                var consumer = Session.CreateOrGetConsumer(request.SenderId);
                consumer.SetClientStatus(ClientStatus.wait); // change client status as wait, waiting for pubish ready status.
                Session.MessageManager.RemovePublishedMessage(consumer.ClientId, request.QueueMessageId); // remove publish message to indicate this message has been consumed
                var response = ConsumerMessageBuilder.BuildConsumerResponseMessage(
                    topic: request.Topic,
                    body: request.Body,
                    senderId: request.SenderId
                    );
                response.CanConsume = false; // waiting for the publish message to invoke this consumer
                response.ClientCurrentStatus = consumer.Status;
                return response;
            };
            return callbackAction;
        }

        protected Func<ConsumerRequest, ConsumerResponse> ConsumerDisconnectAction()
        {
            Func<ConsumerRequest, ConsumerResponse> disConnectAction = (request) =>
            {
                this.Session.ClientStrategy.ConsumerDisconnect(request.SenderId); //consume disconnect action
                var response = ConsumerMessageBuilder.BuildConsumerResponseMessage(
                  topic: request.Topic,
                  body: request.Body,
                  senderId: request.SenderId
                  );
                response.ClientCurrentStatus = ClientStatus.disconnect;
                return response;
            };

            return disConnectAction;
        }

        protected Func<ProducerRequest, ProducerResponse> ProducerSendAction()
        {
            Func<ProducerRequest, ProducerResponse> producerAction = (request) =>
            {
                var producer = Session.CreateOrGetProducer(request.SenderId);
                Session.SubscribeProducer(request.Topic, producer);
                this.Session
                    .QueueService
                    .CreateTopicQueuesIfNotExists(request.Topic, defaultQueueCountWithEeachTopic);

                producer.StartWorkingDate = DateTime.Now;

                var queueMessage = QueueMessage.Create(request.Header,request.Body);
                bool sendResult =Session.MessageManager.TrySendMessage(
                    request.Topic,
                    producer.ClientId,
                    queueMessage
                    );
                var storeResult = this.MessageStore.Store(queueMessage);
                var response = ProducerMessageBuilder.BuildResponseMessage(
                    topic: request.Topic,
                    body: request.Body,
                    senderId: request.SenderId
                    );
                response.SendSuccess = sendResult;
                return response;
            };

            return producerAction;
        }

        protected Func<ProducerRequest, ProducerResponse> ProducerDisconnectAction()
        {
            Func<ProducerRequest, ProducerResponse> producerDisconnectAction = (request) =>
            {
                this.Session.ClientStrategy.ProducerDisconnect(request.SenderId);
                var response = ProducerMessageBuilder.BuildResponseMessage(
                     topic: request.Topic,
                     body: request.Body,
                     senderId: request.SenderId
               );
                return response;
            };
            return producerDisconnectAction;
        }

        private void StartClientListener(out Thread th, IClientListener listener)
        {
            th = new Thread(new ThreadStart(listener.StartListen));
            th.IsBackground = true;
            th.Start();
        }

    }
}

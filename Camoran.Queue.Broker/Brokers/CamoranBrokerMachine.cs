using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Client;
using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using Camoran.Queue.Util.Extensions;
using Camoran.Queue.Util.Helper;
using Camoran.Queue.Util.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Brokers
{

    public interface ICamoranBrokerMachine
    {
        void Start();
        void Stop();
        void Initial();
        ICamoranBrokerMachine RegistBrokerSession(ICamoranBrokerSession session);
        ICamoranBrokerMachine InitialClientListener(HostConfig config);
        ICamoranBrokerSession Session { get; }
    }
    public class CamoranBrokerMachine : ICamoranBrokerMachine
    {
        private ICamoranBrokerSession _session;
        public ICamoranBrokerSession Session
        {
            get
            {
                if (_session == null) throw new NullReferenceException("session can't be null or empty, you must regist session first");
                return _session;
            }
            private set { _session = value; }
        }

        protected readonly int QueueCountWithEeachTopic = 5;
        protected readonly int ConsumerTimeoutSeconds = 10;

        private System.Timers.Timer _startQueueScedule = new System.Timers.Timer();
        private System.Timers.Timer _removedTimeoutConsumersScedule = new System.Timers.Timer();
        private static object lockObj = new object();

        public CamoranBrokerMachine(BrokerConfig config = null)
        {
            if (config != null)
            {
                this.QueueCountWithEeachTopic = config.QueueCountforEachTopic;
                this.ConsumerTimeoutSeconds = config.ConsumerTimeoutSeconds;
            }
            Initial();
        }

        public void Initial()
        {
            _startQueueScedule.SetSceduleWork(1, (o, e) => StartQueues());
            _removedTimeoutConsumersScedule.SetSceduleWork(ConsumerTimeoutSeconds * 100, (o, e) =>
              this.Session.ClientBehavior.ConsumerTimeout(ConsumerTimeoutSeconds)
            );
        }

        public void Start()
        {
            try
            {
                _startQueueScedule.Start();
                _removedTimeoutConsumersScedule.Start();
                Session.ConsumerListener.StartListen();
                Session.ProducerListener.StartListen();
            }
            catch
            {
                this.Stop();
            }

        }

        public void Stop()
        {
            _startQueueScedule.Close();
            _startQueueScedule.Dispose();
            _removedTimeoutConsumersScedule.Close();
            _removedTimeoutConsumersScedule.Dispose();
            Session.ConsumerListener.StopListen();
            Session.ProducerListener.StopListen();
        }

        public ICamoranBrokerMachine RegistBrokerSession(ICamoranBrokerSession session)
        {
            if (session == null) throw new ArgumentNullException("session object can't be null or empty");
            this.Session = session;
            return this;
        }

        public ICamoranBrokerMachine InitialClientListener(HostConfig config)
        {
            if (Session == null) throw new NullReferenceException("please regist broker session first");
            if (config == null) throw new NullReferenceException("config can't be null");
            Session.BindListener(config);

            var _consumerListener = Session.ConsumerListener as IClientListener<ConsumerRequest, ConsumerResponse>;
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.consume.ToString(), ConsumerConnectAndConsumeAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.callback.ToString(), ConsumerConsumeCallBackAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.disconnect.ToString(), ConsumerDisconnectAction());

            var _producerListener = Session.ProducerListener as IClientListener<ProducerRequest, ProducerResponse>;
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.send.ToString(), ProducerConnectAndSendAction());
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.disconnect.ToString(), ProducerDisconnectAction());
            return this;
        }

        protected  void StartQueues()
        {
            ThreadHelper.TryLock(lockObj,
                () =>
                {
                    var queues = Session.QueueService.TopicQueues.Select(kv => kv);
                    foreach (var kv in queues)
                    {
                         this.StartQueues(kv.Key, kv.Value);
                    }
                }, null);
        }


        protected virtual Func<ConsumerRequest, ConsumerResponse> ConsumerConnectAndConsumeAction()
        {
            Func<ConsumerRequest, ConsumerResponse> consumeAction = (request) =>
            {
                Session.ClientBehavior.ConsumerConnect(request.SenderId);
                var consumer = Session.CreateOrGetConsumer(request.SenderId);
                Session.SubscribeConsumer(request.Topic, consumer);
                this.Session
                    .QueueService
                    .CreateTopicQueuesIfNotExists(request.Topic, QueueCountWithEeachTopic);
                consumer.StartWorkingDate = DateTime.Now;
                var canConsume = consumer.Status == ClientStatus.ready;
                if (canConsume)
                {
                    consumer.SetClientStatus(ClientStatus.working);
                }
                var response = Session.MessageManager.ConsumerMessageBuilder.BuildConsumerResponseMessage(
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
                    response.FromQueueId = consumingMessage.QueueId;
                }
                return response;
            };

            return consumeAction;
        }
        protected virtual Func<ConsumerRequest, ConsumerResponse> ConsumerConsumeCallBackAction()
        {
            Func<ConsumerRequest, ConsumerResponse> callbackAction = (request) =>
            {
                var consumer = Session.CreateOrGetConsumer(request.SenderId);
                consumer.SetClientStatus(ClientStatus.wait); // change client status as wait, waiting for pubish ready status.
                Session.MessageManager.RemovePublishedMessage(consumer.ClientId, request.QueueMessageId); // remove publish message to indicate this message has been consumed
                var response = Session.MessageManager.ConsumerMessageBuilder.BuildConsumerResponseMessage(
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
        protected virtual Func<ConsumerRequest, ConsumerResponse> ConsumerDisconnectAction()
        {
            Func<ConsumerRequest, ConsumerResponse> disConnectAction = (request) =>
            {
                this.Session.ClientBehavior.ConsumerDisconnect(request.SenderId); //consume disconnect action
                var response = Session.MessageManager.ConsumerMessageBuilder.BuildConsumerResponseMessage(
                  topic: request.Topic,
                  body: request.Body,
                  senderId: request.SenderId
                  );
                response.ClientCurrentStatus = ClientStatus.disconnect;
                return response;
            };

            return disConnectAction;
        }
        protected virtual Func<ProducerRequest, ProducerResponse> ProducerConnectAndSendAction()
        {
            Func<ProducerRequest, ProducerResponse> producerAction = (request) =>
            {
                Session.ClientBehavior.ProducerConnect(request.SenderId);
                var producer = Session.CreateOrGetProducer(request.SenderId);
                Session.SubscribeProducer(request.Topic, producer);
                this.Session
                    .QueueService
                    .CreateTopicQueuesIfNotExists(request.Topic, QueueCountWithEeachTopic);

                producer.StartWorkingDate = DateTime.Now;

                var queueMessage = QueueMessage.Create(request.Header, request.Body, request.Topic);
                bool sendResult = Session.MessageManager.TrySendMessage(
                    request.Topic,
                    producer.ClientId,
                    queueMessage
                    );
                var storeResult = this.Session.MessageManager.MessageStore.Store(queueMessage); // how about strore failure? log or send message  to producer?
                var response = Session.MessageManager.ProducerMessageBuilder.BuildResponseMessage(
                    topic: request.Topic,
                    body: request.Body,
                    senderId: request.SenderId
                    );
                response.SendSuccess = sendResult;
                return response;
            };

            return producerAction;
        }
        protected virtual Func<ProducerRequest, ProducerResponse> ProducerDisconnectAction()
        {
            Func<ProducerRequest, ProducerResponse> producerDisconnectAction = (request) =>
            {
                this.Session.ClientBehavior.ProducerDisconnect(request.SenderId);
                var response = Session.MessageManager.ProducerMessageBuilder.BuildResponseMessage(
                     topic: request.Topic,
                     body: request.Body,
                     senderId: request.SenderId
               );
                return response;
            };
            return producerDisconnectAction;
        }
        private  void StartQueues(string topic, IEnumerable<MessageQueue> topicQueues)
        {
                 this.Session.QueueService.StartQueues(
                   topicQueues,
                  (queue) =>
                  {
                      var consumer = this.Session.ConsumerManager.FindConsumerByQueueId(topic, queue.QueueId);
                      return consumer == null ? false : consumer.Status == ClientStatus.wait;
                  },
                  (queueMessage) =>
                  {
                      this.Session.MessageManager.PublishMessage(queueMessage.Topic, queueMessage);
                  });
        }

    }
}

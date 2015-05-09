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

        protected readonly int defaultQueueCountWithEeachTopic = 200;

        protected readonly int consumerTimeout = 20;

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
            _removedConsumersScedule.SetSceduleWork(consumerTimeout * 1000, (o, e) => ConsumerTimeout(consumerTimeout));
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
            this.Session = session;
            return this;
        }


        public CamoranBrokerMachine InitialClientListener()
        {
            _consumerListener = Session.ConsumerListener as CamoranConsumerListener;
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.consume.ToString(), ConsumerConsumeAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.callback.ToString(), ConsumerConsumeCallBackAction());
            _consumerListener.ReceiveEvents.GetOrAdd(ConsumerRequestType.disconnect.ToString(), ConsumerDisconnectAction());

            _producerListener = Session.ProducerListener as CamoranProducerListener;
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.send.ToString(), ProducerSendAction());
            _producerListener.ReceiveEvents.GetOrAdd(ProducerRequestType.disconnect.ToString(), ProducerDisconnectAction());
            return this;
        }

        public CamoranBrokerMachine AddConsumerListenerEvent(string eventType, Func<ConsumerRequest, ConsumerResponse> action)
        {
            (this.Session.ConsumerListener as CamoranConsumerListener)
                .ReceiveEvents
                .TryAdd(eventType, action);
            return this;
        }

        public CamoranBrokerMachine AddProducerLisenerEvent(string eventType, Func<ProducerRequest, ProducerResponse> action)
        {
            (this.Session.ConsumerListener as CamoranProducerListener)
                   .ReceiveEvents
                   .TryAdd(eventType, action);
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

        protected virtual void ProducerDisconnect(Guid producerid)
        {
            var dissConnectProducer = Session.CreateOrGetProducer(producerid);
            Session.ClientManager.RemoveProducer(dissConnectProducer);
        }

        protected virtual void ProducerTimeout(int timeoutSeconds)
        {
            var timeoutProducers = Session.ClientManager.FindTimeoutProducers(timeoutSeconds);
            Session.ClientManager.RemoveProducers(timeoutProducers);
        }

        protected virtual void ConsumerDisconnect(Guid consumerId)
        {
            bool messageExists = false;
            var publishMessages = Session.GetPublishMessagesByConsumerId(consumerId, out messageExists);
            if (messageExists)
                this.ReEnqueueMessages(publishMessages);

            var disConnectConsumer = Session.CreateOrGetConsumer(consumerId);
            Session.RemovePublishMessagesByConsumer(disConnectConsumer);
            Session.ClientManager.RemoveConsumer(disConnectConsumer);
        }

        protected virtual void ConsumerTimeout(int timeoutSeconds)
        {
            lock (lockObj)
            {
                /*check all consumers status if status not wait and out of timeout range then remove this consumer and re-enqueue message */
                var timeoutConsumers = Session.ClientManager.FindTimeoutConsumers(timeoutSeconds);
                if (timeoutConsumers == null || timeoutConsumers.Count() <= 0) return;
                // re-enqueue timeout messages
                var timeoutMessages = Session.GetPublishMessagesByConsumers(timeoutConsumers);
                this.ReEnqueueMessages(timeoutMessages);
                // remove published messages 
                Session.RemovePublishMessagesByConsumers(timeoutConsumers);
                Session.ClientManager.RemoveConsumers(timeoutConsumers);
            }
        }

        protected void PublishMessage(string topic, QueueMessage message)
        {
            var consumer = this.FindConsumer(topic, message.QueueId);
            if (consumer != null)
            {
                consumer.SetClientStatus(ClientStatus.ready);
                consumer.CurrentQueueMessage = message;

                Session.PublishedMessages.AddOrUpdate(
                    consumer.ClientId,
                    new List<QueueMessage> { message },
                    (key, old) =>
                    {
                        old.Add(message);
                        return old;
                    });
            }
        }

        protected bool SendMessageToQueue(string topic, Guid senderId, QueueMessage message)
        {
            IList<CamoranProducer> producers = null;
            IList<MessageQueue> queues = null;
            var queuesExits = Session.TopicQueues.TryGetValue(topic, out queues);
            if (!queuesExits) return false;
            Session.MappingListBetweenTopicAndProducers.TryGetValue(topic, out producers);
            int producerIndex = this.GetIndexInList(producers
                , (producer, id) => producer.ClientId == id
                , senderId);// find index in producers
            int queueIndx = this.FindQueueIndex(queues.Count, producerIndex);
            var queue = queues[queueIndx];
            queue.Enqueue(message);
            return true;

        }

        protected virtual int FindConsumerIndex(int queueCount, int consumerCount, int queueIndex)
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

        protected virtual int FindQueueIndex(int queueCount, int producerIndex)
        {
            return producerIndex.GetHashCode()
                % queueCount;
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
                          var consumer = this.FindConsumer(topic, queue.QueueId);
                          return consumer == null ? false : consumer.Status == ClientStatus.wait;
                      },
                      (queueMessage) =>
                      {
                          this.PublishMessage(topic, queueMessage);
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
                Session.RemovePublishedMessage(consumer.ClientId, request.QueueMessageId); // remove publish message to indicate this message has been consumed
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
                this.ConsumerDisconnect(request.SenderId); //consume disconnect action
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
                bool sendResult = this.SendMessageToQueue(
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
                this.ProducerDisconnect(request.SenderId);
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

        private void ReEnqueueMessages(IEnumerable<QueueMessage> messages)
        {
            var queues = Session.TopicQueues.Values.SelectMany(queue => queue);

            var queueWithMessages =
                      from q in queues
                      from m in messages
                      where q.QueueId == m.QueueId
                      select new { Queue = q, Message = m };

            foreach (var item in queueWithMessages)
            {
                item.Queue.Enqueue(item.Message);
            }
        }

        private CamoranConsumer FindConsumer(string topic, Guid fromQueueId)
        {
            IList<CamoranConsumer> consumers = null;
            IList<MessageQueue> queues = null;
            bool hasConsumers = Session.MappingListBetweenTopicAndConsumers.TryGetValue(topic, out consumers);
            bool hasQueues = Session.TopicQueues.TryGetValue(topic, out queues);

            if (!hasConsumers || consumers.Count <= 0) return null;
            if (!hasQueues) throw new ApplicationException("queues should exist!");

            var queueIndex = this.GetIndexInList(
                  queues,
                  (queue, id) => queue.QueueId == id,
                  fromQueueId);
            var consumerIndex = this.FindConsumerIndex(queues.Count, consumers.Count, queueIndex);
            var consumer = consumers[consumerIndex];

            return consumer;
        }

        private int GetIndexInList<T, V>(IList<T> ls, Func<T, V, bool> condiftion, V para)
        {
            return ls.ToList().GetFirstIndex(condiftion, para);
        }
    }
}

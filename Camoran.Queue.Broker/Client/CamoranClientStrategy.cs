using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Client
{
    public class CamoranClientStrategy : ICamoranClientStrategy
    {
        protected ICamoranBrokerSession Session { get; private set; }

        public CamoranClientStrategy(ICamoranBrokerSession session)
        {
            this.Session = session;
        }

        public virtual void ProducerDisconnect(Guid producerid)
        {
            var dissConnectProducer = Session.CreateOrGetProducer(producerid);
            Session.ClientManager.RemoveProducer(dissConnectProducer);
        }

        public virtual void ProducerTimeout(int timeoutSeconds)
        {
            var timeoutProducers = Session.ClientManager.FindTimeoutProducers(timeoutSeconds);
            Session.ClientManager.RemoveProducers(timeoutProducers);
        }

        public virtual void ConsumerDisconnect(Guid consumerId)
        {
            bool messageExists = false;
            var publishMessages = Session.MessageManager.GetPublishMessagesByConsumerId(consumerId, out messageExists);
            if (messageExists)
                this.ReEnqueueMessages(publishMessages);

            var disConnectConsumer = Session.CreateOrGetConsumer(consumerId);
            Session.MessageManager.RemovePublishMessagesByConsumerId(disConnectConsumer.ClientId);
            Session.ClientManager.RemoveConsumer(disConnectConsumer);
        }

        public virtual void ConsumerTimeout(int timeoutSeconds)
        {
            lock (this)
            {
                /*check all consumers status if status not wait and out of timeout range then remove this consumer and re-enqueue message */
                var timeoutConsumers = Session.ClientManager.FindTimeoutConsumers(timeoutSeconds);
                if (timeoutConsumers == null || timeoutConsumers.Count() <= 0) return;
                // re-enqueue timeout messages
                var timeoutMessages = this.GetPublishMessagesByConsumers(timeoutConsumers);
                this.ReEnqueueMessages(timeoutMessages);
                // remove published messages 
                this.RemovePublishMessagesByConsumers(timeoutConsumers);
                Session.ClientManager.RemoveConsumers(timeoutConsumers);
            }
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

        private IList<QueueMessage> GetPublishMessagesByConsumers(IEnumerable<Camoran.Queue.Client.Consumer.CamoranConsumer> consumers)
        {
            List<QueueMessage> queueMessages = new List<QueueMessage>();
            bool messageExists = false;
            foreach (var consumer in consumers)
            {
                var messages = Session.MessageManager.GetPublishMessagesByConsumerId(consumer.ClientId, out messageExists);
                if (messageExists)
                    queueMessages.AddRange(messages);
            }
            return queueMessages;
        } 

        private void RemovePublishMessagesByConsumers(IEnumerable<Camoran.Queue.Client.Consumer.CamoranConsumer> removedConsumers)
        {
            foreach (var consumer in removedConsumers)
            {
                Session.MessageManager.RemovePublishMessagesByConsumerId(consumer.ClientId);
            }
        }


    }
}

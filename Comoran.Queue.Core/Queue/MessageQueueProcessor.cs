using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Queue
{
    public abstract class MessageQueueProcessor : IMessageQueueProcessor<MessageQueue,QueueMessage>
    {
        public abstract Task ProcessQueueAsync(MessageQueue queue,Func<bool> waitingSwitch, Action<QueueMessage> processCallBack);
    }
}

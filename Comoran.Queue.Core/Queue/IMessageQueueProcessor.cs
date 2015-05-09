using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Queue
{
    public interface IMessageQueueProcessor<Queue,Message> 
        where Queue : IMessageQueue 
        where Message:IMessage
    {
        Task ProcessQueueAsync(Queue queue,Func<bool> waitingSwitch,Action<Message> processCallBack);
    }
}

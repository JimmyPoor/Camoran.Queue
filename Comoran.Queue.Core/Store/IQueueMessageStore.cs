using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Store
{
    public interface IQueueMessageStore:IMessageStore<QueueMessage,QueueMessageStoreResult>
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Store
{
    public abstract class QueueMessageStore : IQueueMessageStore
    {
        public abstract QueueMessageStoreResult Store(Message.QueueMessage msg);
    }
}

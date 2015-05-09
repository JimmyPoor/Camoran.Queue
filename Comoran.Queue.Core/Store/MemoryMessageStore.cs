using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Store
{
    public class MemoryMessageStore : QueueMessageStore
    {
        
        public override QueueMessageStoreResult Store(Message.QueueMessage msg)
        {
            return new QueueMessageStoreResult(true,msg);
        }
    }
}

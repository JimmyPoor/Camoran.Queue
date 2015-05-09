using Camoran.Queue.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Store
{
    public class QueueMessageRedisStore : QueueMessageStore
    {
        public override QueueMessageStoreResult Store(Core.Message.QueueMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}

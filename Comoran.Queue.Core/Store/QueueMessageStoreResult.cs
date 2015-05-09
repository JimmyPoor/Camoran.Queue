using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Store
{
    public class QueueMessageStoreResult : MessageStoreResult<QueueMessage>
    {
        public QueueMessageStoreResult(bool storeSuccess, QueueMessage msg)
            : base(storeSuccess, msg)
        {
        }
    }
}

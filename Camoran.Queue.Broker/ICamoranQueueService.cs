using Camoran.Queue.Core;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker
{
    public interface ICamoranQueueService : IQueueService<MessageQueue>
    {
        IList<MessageQueue> CreateTopicQueuesIfNotExists(string topic, int createQueueCount);
    }

}

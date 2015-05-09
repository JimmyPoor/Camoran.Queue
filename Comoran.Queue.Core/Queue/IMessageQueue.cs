using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Queue
{
    public interface IMessageQueue:IQueue<QueueMessage>
    {
        QueueMessage[] Messages { get; }
        Guid QueueId { get; }
        QueueStatus QueueStatus { get; }
        DateTime CreateDate { get; }

        int CurrentOffset { get; }
        int Size();
        void Clear();
        void SetAllowMessageSize(int allowSize);
        void SetIntialMessageSize(int intialSize);
        void SetQueueStatus(QueueStatus status);

    }
}

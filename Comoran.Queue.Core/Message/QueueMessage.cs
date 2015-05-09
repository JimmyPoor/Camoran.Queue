using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Message
{
    public sealed class QueueMessage : IMessage
    {
       
        public QueueMessage(string header)
            : this(header, null)
        {

        }
        public QueueMessage(string header, byte[] body)
            : this(header, body, Guid.Empty)
        {

        }

        public QueueMessage(string header, byte[] body, Guid queueId)
        {
            this.Header = header;
            this.Body = body;
            this.QueueId = queueId;
            this.CreateDate = DateTime.Now;
            this.MessageId = Guid.NewGuid();
        }

        public void SetQueueId(Guid queueId)
        {
            this.QueueId = queueId;
        }

        public void SetOffset(int offset)
        {
            this.QueueOffset = offset;
        }

        public static QueueMessage Create(string header, byte[] body)
        {
            return new QueueMessage(header,body);
        }

        public Guid MessageId { get; private set; }
        public int QueueOffset { get; private set; }
        public Guid QueueId { get; private set; }
        public string Header { get; set; }
        public byte[] Body { get; set; }
        public DateTime CreateDate { get; private set; }

    }

    public class QueueMessageCompire : IEqualityComparer<QueueMessage>
    {
        public bool Equals(QueueMessage x, QueueMessage y)
        {
            return x.QueueId == y.QueueId;
        }

        public int GetHashCode(QueueMessage obj)
        {
            return obj.GetHashCode();
        }
    }
}

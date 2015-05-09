using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Queue
{
    public class MessageQueue : IMessageQueue
    {
        protected int allowedMessageSize = 100000;

        private int rearIndex = 0;
        private int frontIndex = 1;

        private object lockObj = new object();
        public MessageQueue(Guid queueId) 
        {
            this.QueueId = queueId;
            this.CreateDate = DateTime.Now;
            Messages = new QueueMessage[allowedMessageSize];
        }

        public QueueMessage[] Messages { get; private set; }

        public Guid QueueId { get; private set; }

        public QueueStatus QueueStatus { get; private set; }

        public int CurrentOffset { get; protected set; }

        public DateTime CreateDate
        {
            get;
            private set;
        }


        public virtual void Enqueue(QueueMessage message)
        {
            if (message == null)
            {
                return;
            }
            lock (lockObj)
            {
                rearIndex = (rearIndex + 1) % this.Messages.Length;
                if (rearIndex == this.allowedMessageSize) return;
                Messages[rearIndex] = message;
                if (QueueIsFull())
                {
                    QueueMessage[] temp = new QueueMessage[rearIndex + 2];
                    Array.Copy(Messages, 0, temp, 0, temp.Length - 1);
                    Messages = temp;
                }
                CurrentOffset = rearIndex;
                message.SetQueueId(this.QueueId); // recorde queueId in queue message
                message.SetOffset(this.CurrentOffset);//recode queue current offset 
            }


        }


        public virtual QueueMessage Dequeue()
        {

            lock (lockObj)
            {
                if (QueueIsEmpty() )
                {
                    return null;
                }
                QueueMessage[] temp = new QueueMessage[rearIndex + 1];
                QueueMessage message = Messages[frontIndex];
                Array.Copy(Messages, frontIndex + 1, temp, frontIndex, rearIndex);
                Messages = temp;
                this.rearIndex -= 1;
                CurrentOffset = frontIndex;
                message.SetOffset(this.CurrentOffset);//recode queue current offset
                return message;
            }

        }

        public virtual int Size()
        {
            return this.rearIndex;
        }

        public virtual void Clear()
        {
            if (this.Messages.Length > 0)
                Array.Clear(this.Messages, 0, this.Messages.Length);
        }

        public virtual void SetAllowMessageSize(int allowSize)
        {
            this.allowedMessageSize = allowSize;
        }

        public virtual void SetQueueStatus(QueueStatus status)
        {
            this.QueueStatus = status;
        }

        public void SetIntialMessageSize(int intialSize)
        {
            Messages = new QueueMessage[intialSize];
        }

        private bool QueueIsEmpty()
        {
            return (rearIndex + 1) % Messages.Length == frontIndex || rearIndex <= 0;
        }

        private bool QueueIsFull()
        {
            return (rearIndex + 2) % Messages.Length == frontIndex;
        }

        private bool CompareChange(ref int location, int newValue, int compaire)
        {
            return compaire == Interlocked.CompareExchange(ref location, newValue, compaire);
        }


    }

   

}
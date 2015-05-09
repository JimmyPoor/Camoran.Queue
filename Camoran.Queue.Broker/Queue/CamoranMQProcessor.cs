using Camoran.Queue.Core.Message;
using Camoran.Queue.Core.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Queue
{

    public class CamoranMQProcessor : MessageQueueProcessor
    {
        private object lockobj = new object();
        public override async Task ProcessQueueAsync(MessageQueue queue, Func<bool> waitingSwitch, Action<QueueMessage> processCallBack)
        {
            if (queue == null) throw new ArgumentNullException("Queue is null");

            await Task.Run(() =>
            {
                while (true)
                {
                    lock (lockobj)
                    {
                        if (queue.Size() > 0)
                        {
                            if (waitingSwitch == null || waitingSwitch.Invoke())
                            {
                                QueueMessage message = queue.Dequeue();
                                processCallBack.Invoke(message);
                            }
                        }
                    }
  
                    Thread.Sleep(20);
                }
            });

        }
    }
}

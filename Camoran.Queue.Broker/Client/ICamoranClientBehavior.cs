using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Client
{
    public interface ICamoranClientBehavior
    {
        void ConsumerConnect(Guid consumerId);
        void ProducerConnect(Guid producerId);
        void ConsumerDisconnect(Guid consumerId);
        void ProducerDisconnect(Guid producerid);
        void ProducerTimeout(int seconds);
        void ConsumerTimeout(int seconds);
    }
}

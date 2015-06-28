using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Client
{
    public interface ICamoranClientStrategy
    {
        void ConsumerDisconnect(Guid consumerId);
        void ProducerDisconnect(Guid producerid);
        void ProducerTimeout(int timeoutSeconds);
        void ConsumerTimeout(int timeoutSeconds);
    }
}

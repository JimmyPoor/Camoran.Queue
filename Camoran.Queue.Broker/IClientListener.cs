using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker
{
    public interface IClientListener
    {
        void StartListen();
        void StopListen();
    }
}

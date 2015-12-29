using Camoran.Queue.Client;
using System;
using System.Collections.Concurrent;
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

    public interface IClientListener<Request, Response>: IClientListener
          where Request : ClientMessage
        where Response : ClientMessage
    {
        ConcurrentDictionary<string, Func<Request, Response>> ReceiveEvents { get; set; }
    }
}

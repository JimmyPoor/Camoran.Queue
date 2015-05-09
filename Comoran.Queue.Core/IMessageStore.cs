using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core
{
    public interface IMessageStore<Message,StoreResult>
    {
        StoreResult Store(Message msg);
    }


      
}

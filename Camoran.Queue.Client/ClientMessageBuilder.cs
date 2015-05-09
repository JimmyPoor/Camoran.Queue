using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client
{
    public interface IClientMessageBuilder<Message> where Message:IClientMessage
    {
        Message Build();
    }
}

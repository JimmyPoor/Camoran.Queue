﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core
{
    public interface IQueue<Message>
    {

       void Enqueue(Message message);
       Message Dequeue();
    }
}

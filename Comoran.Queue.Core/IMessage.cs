using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core
{
    public interface IMessage
    {
        string Header { get; set; }

        byte[] Body { get; set; }

        DateTime CreateDate { get;  }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Core.Store
{
    public class MessageStoreResult<Message> where Message: IMessage
    {
        public bool IsSuccess { get; private set; }

        public Message CurrentMsg { get; private set; }


        public MessageStoreResult(bool isSuccess,Message msg) 
        {
            this.IsSuccess = isSuccess;
            this.CurrentMsg = msg;
        }
    }
}

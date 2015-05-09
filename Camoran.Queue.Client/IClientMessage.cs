using Camoran.Queue.Core;
using Camoran.Queue.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client
{
    public interface IClientMessage : IMessage
    {

        string Topic { get; }

        ErrorMessage Error { get; set; }

        Guid SenderId { get; set; }

        IDictionary<string, object> Params { get; set; }

        void SetCreateDate(DateTime createDate);

        void SetReceiveDate(DateTime receiveDate);

        string MessageType { set; get; }

        DateTime ReceiveDate { get; }

        //void SetTopic(string topic);
    }
}

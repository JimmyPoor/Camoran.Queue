
using Camoran.Queue.Core.Message;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Consumer
{
    [ProtoContract]
    public class ConsumerRequest : ClientMessage
    {

        public ConsumerRequest()
        {
        }

        [ProtoMember(41)]
        public Guid QueueMessageId { get; set; }
        public ConsumerRequest(string topic,byte[] body,Guid senderId,ConsumerRequestType requestType)
            : base(topic, body, senderId)
        {
            this.MessageType = requestType.ToString();
        }
        public override void SetCreateDate(DateTime createDate)
        {
            this.CreateDate = createDate;
        }

    }


    public enum ConsumerRequestType
    {
        connect,
        consume,
        callback,
        disconnect
    }
}

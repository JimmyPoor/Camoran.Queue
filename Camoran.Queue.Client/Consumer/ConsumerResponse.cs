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
    public class ConsumerResponse : ClientMessage
    {
        public ConsumerResponse() { }
        [ProtoMember(10)]
        public bool CanConsume { get; set; }
        [ProtoMember(11)]
        public ClientStatus ClientCurrentStatus { get; set; }
        [ProtoMember(12)]
        public Guid QueueMessageId { get; set; }
        [ProtoMember(13)]
        public byte[] QueueMeesageBody { get; set; }
        //[ProtoMember(14)]
        //public int QueueCurrentOffset { get; set; }

        public ConsumerResponse(string topic,byte[] body, Guid senderId)
            : base(topic, body, senderId)
        {
            
        }

        public override void SetCreateDate(DateTime createDate)
        {
            base.SetCreateDate(createDate);
        }

        public override void SetReceiveDate(DateTime receiveDate)
        {
            base.SetReceiveDate(receiveDate);
        }



    }
}

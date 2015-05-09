using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Producer
{
    [ProtoContract]
    public class ProducerRequest : ClientMessage
    {
        public ProducerRequest() { }
        public ProducerRequest(string topic, byte[] body, Guid senderId, ProducerRequestType requestType)
            : base(topic, body, senderId)
        {
            this.MessageType = requestType.ToString();
        }
        public override void SetCreateDate(DateTime createDate)
        {
            base.SetCreateDate(createDate);
        }

    }


    public enum ProducerRequestType
    {
        send,
        disconnect,
        //request
    }
}

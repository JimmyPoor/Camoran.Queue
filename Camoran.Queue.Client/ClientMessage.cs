using Camoran.Queue.Client.Consumer;
using Camoran.Queue.Client.Producer;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Camoran.Queue.Client
{
    [ProtoContract]
    [ProtoInclude(30, typeof(ProducerRequest))]
    [ProtoInclude(31, typeof(ProducerResponse))]
    [ProtoInclude(32, typeof(ConsumerRequest))]
    [ProtoInclude(33, typeof(ConsumerResponse))]
    public abstract class ClientMessage : IClientMessage
    {
        [ProtoMember(1)]
        public string Topic { get; protected set; }

        [ProtoMember(2)]
        public IDictionary<string, object> Params { get; set; }

        // [ProtoMember(3)]
        public Camoran.Queue.Core.Message.ErrorMessage Error { get; set; }

        [ProtoMember(4)]
        public Guid SenderId { get; set; }
        [ProtoMember(5)]
        public DateTime ReceiveDate { get; protected set; }
        [ProtoMember(6)]
        public string Header { get; set; }
        [ProtoMember(7)]
        public byte[] Body { get; set; }
        [ProtoMember(8)]
        public virtual DateTime CreateDate { get; protected set; }
        [ProtoMember(9)]
        public virtual string MessageType { get; set; }


        public ClientMessage() { }
        public ClientMessage(string topic, byte[] body, Guid senderId)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic can't be null or empty");
            this.Topic = topic;
            this.SenderId = senderId;
            this.CreateDate = DateTime.Now;
            this.Params = new Dictionary<string, object>();
            this.Body = body;
        }


        public virtual void SetCreateDate(DateTime createDate)
        {
            this.CreateDate = createDate;
        }

        public virtual void SetReceiveDate(DateTime receiveDate)
        {
            this.ReceiveDate = receiveDate;
        }
    }
}

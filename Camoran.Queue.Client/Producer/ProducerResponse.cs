using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Producer
{
    [ProtoContract]
    public class ProducerResponse : ClientMessage
    {

        public ProducerResponse() { }

        [ProtoMember(21)]
        public bool SendSuccess { get; set; }


        public ProducerResponse(string topic, byte[] body, Guid senderId)
            : base(topic, body, senderId)
        {
            
        }

    }
}

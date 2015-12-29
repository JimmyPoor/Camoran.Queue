using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Consumer
{
    public interface IConsumerMessageBuilder : 
        IClientMessageBuilder<ConsumerRequest>,
        IClientMessageBuilder<ConsumerResponse>
    {
        ConsumerRequest BuildConsumerRequestMessage(string topic,byte[] body,Guid senderId,ConsumerRequestType requestType);
        ConsumerResponse BuildConsumerResponseMessage(string topic, byte[] body, Guid senderId);
    }
    public class ConsumerMessageBuilder : IConsumerMessageBuilder
    {
        public virtual ConsumerRequest BuildConsumerRequestMessage(string topic, byte[] body, Guid senderId, ConsumerRequestType requestType)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic null");
            if (senderId == Guid.Empty) throw new ArgumentException("senderid null");
            return new ConsumerRequest(topic, body, senderId, requestType);
        }

        public virtual ConsumerResponse BuildConsumerResponseMessage(string topic, byte[] body, Guid senderId)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic null");
            if (senderId == Guid.Empty) throw new ArgumentException("senderid null");
            return new ConsumerResponse(topic, body, senderId);
        }

        ConsumerRequest IClientMessageBuilder<ConsumerRequest>.Build()
        {
            return new ConsumerRequest();
        }

        ConsumerResponse IClientMessageBuilder<ConsumerResponse>.Build()
        {
            return new ConsumerResponse();
        }
    }
}

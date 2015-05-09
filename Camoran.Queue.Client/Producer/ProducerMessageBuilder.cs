using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Producer
{
    public interface IProducerMessageBuilder : 
        IClientMessageBuilder<ProducerRequest>,
        IClientMessageBuilder<ProducerResponse> 
    {
        ProducerRequest  BuildSendRequestMessage(string topic, byte[] body, Guid senderId, ProducerRequestType requestType);
        ProducerResponse BuildResponseMessage(string topic,byte[] body,Guid senderId);
    }
    public class ProducerMessageBuilder : IProducerMessageBuilder
    {
        public ProducerRequest Build()
        {
            return new ProducerRequest();
        }

        ProducerResponse IClientMessageBuilder<ProducerResponse>.Build()
        {
            return new ProducerResponse();
        }

        public ProducerRequest BuildSendRequestMessage(string topic, byte[] body, Guid senderId, ProducerRequestType requestType)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic value not allow be null");
            if (senderId == Guid.Empty) throw new ArgumentNullException("sender id not allow be null");
            return new ProducerRequest(topic, body, senderId, requestType);
        }


        public ProducerResponse BuildResponseMessage(string topic, byte[] body, Guid senderId)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic value not allow be null");
            if (senderId == Guid.Empty) throw new ArgumentNullException("sender id not allow be null");
            return new ProducerResponse(topic,body,senderId);
        }
    }
}

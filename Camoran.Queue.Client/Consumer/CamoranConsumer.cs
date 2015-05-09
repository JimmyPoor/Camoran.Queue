using Camoran.Queue.Core;
using Camoran.Queue.Util.Serialize;
using Camoran.Socket.Client;
using Camoran.Socket.Message;
using Camoran.Queue.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Consumer
{


    public class CamoranConsumer :
        Client<ConsumerRequest, ConsumerResponse>
    {
        public IConsumerMessageBuilder ConsumeMessageBuilder { get; private set; }

        private string _currentTopic;
        private byte[] _currentBody;
        private Action<ConsumerResponse> _consumeTask;

        private readonly int _consumeSceduleInterval = 100;
        private object _lock = new object();
        private bool _isStart = true;

        public string CurrentTopic { get { return _currentTopic; } }
        public Camoran.Queue.Core.Message.QueueMessage CurrentQueueMessage { get; set; }

        public CamoranConsumer(Guid clientId, HostConfig config)
            : base(clientId, config)
        {
            ConsumeMessageBuilder = new ConsumerMessageBuilder();
        }

        public override void ConnectToServer()
        {
            SocketClient.ConnectToServer(this.Config.Address, this.Config.Port);
        }

        public override ConsumerResponse SendRequest(ConsumerRequest request)
        {
            return base.SendRequest(request);
        }

        public CamoranConsumer SubscribeTopic(string topic)
        {
            this._currentTopic = topic;
            return this;
        }

        public CamoranConsumer SetMessageBody(byte[] body)
        {
            this._currentBody = body;
            return this;
        }

        public CamoranConsumer RegisteConsumeAction(Action<ConsumerResponse> consumeTask)
        {
            this._consumeTask = consumeTask;
            return this;
        }

        public override void Start()
        {
            this.ConnectToServer();
            _isStart = true;
            StartWork();
        }


        public override void Stop()
        {
            _isStart = false;
        }

        public override void Close()
        {
            this.SendConsumerDisconnectRequest();
            //log success or fail
        }

        private void StartWork()
        {
            while (_isStart)
            {
                Thread.Sleep(_consumeSceduleInterval);
                var consumeResponse = this.SendConsumeRequest();
                if (consumeResponse.CanConsume)
                {
                    this._consumeTask.Invoke(consumeResponse);
                    this.SendConsumerCallbackRequest(consumeResponse.QueueMessageId);
                    continue;
                }
            }

        }

        private ConsumerResponse SendConsumeRequest()
        {
            var request = this.CreateRequestByRequestType(ConsumerRequestType.consume);
            var response = this.SendRequest(request);
            return response;
        }

        private ConsumerResponse SendConsumerCallbackRequest(Guid queueMessageId)
        {
            var request = this.CreateRequestByRequestType(ConsumerRequestType.callback);
            request.QueueMessageId = queueMessageId;
            var response = this.SendRequest(request);
            return response;
        }


        private ConsumerResponse SendConsumerDisconnectRequest()
        {
            var request = this.CreateRequestByRequestType(ConsumerRequestType.disconnect);
            var response = this.SendRequest(request);
            return response;
        }


        private ConsumerRequest CreateRequestByRequestType(ConsumerRequestType requestType)
        {
            var consumeRequest = this
          .ConsumeMessageBuilder
          .BuildConsumerRequestMessage(_currentTopic, _currentBody, this.ClientId, requestType);
            return consumeRequest;
        }


    }
}

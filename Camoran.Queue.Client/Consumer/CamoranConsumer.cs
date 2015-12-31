using Camoran.Queue.Core;
using Camoran.Queue.Util.Serialize;
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


    public class CamoranConsumer : Client<ConsumerRequest, ConsumerResponse>
    {
        public IConsumerMessageBuilder ConsumeMessageBuilder { get; private set; }
        private byte[] _currentBody;
        private Action<ConsumerResponse> _consumeTask;
        IClient<ConsumerRequest, ConsumerResponse> _inner;

        private readonly int _consumeSceduleInterval =80;
        private object lockobj = new object();

        public Camoran.Queue.Core.Message.QueueMessage CurrentQueueMessage { get; set; }

        private System.Timers.Timer _consumerTimer = new System.Timers.Timer();
        public CamoranConsumer(Guid clientId, ClientConfig config, IClient<ConsumerRequest, ConsumerResponse> inner)
            : base(clientId, config)
        {
            ConsumeMessageBuilder = new ConsumerMessageBuilder();
            this.SetSceduleWork();
            this._inner = inner;
            this._inner.OnClientFailtoConnect = WhenConsumerConnectFail;
        }



        public override void ConnectToServer()
        {
            _inner.ConnectToServer();
        }

        public override ConsumerResponse SendRequest(ConsumerRequest request)
        {
            return _inner.SendRequest(request);
        }

        public CamoranConsumer SubscribeTopic(string topic)
        {
            base.CurrentTopic = topic;
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
            _consumerTimer.Start();
            _inner.Start();
        }

        public override void Stop()
        {
            _consumerTimer.Stop();
            _inner.Stop();
        }

        public override void Close()
        {
            this.SendConsumerDisconnectRequest();
            _inner.Close();
        }

        protected virtual ConsumerResponse WhenConsumerConnectFail(ConsumerRequest request)
        {
            // record events for fail to connect to server successfully
            return null;
        }

        protected virtual void SetSceduleWork()
        {
            _consumerTimer.SetSceduleWork(_consumeSceduleInterval, (o, b) =>
            {
                Util.Helper.ThreadHelper.TryLock(lockobj, () =>
                {
                    var request = this.CreateRequestByRequestType(ConsumerRequestType.consume);
                    var response = this.SendRequest(request);
                    if (response != null && response.CanConsume)
                    {
                        this._consumeTask.Invoke(response);
                        this.SendConsumerCallbackRequest(response.QueueMessageId);
                    }
                }, null);
            });
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
          .BuildConsumerRequestMessage(base.CurrentTopic, _currentBody, this.ClientId, requestType);
            return consumeRequest;
        }

    }
}

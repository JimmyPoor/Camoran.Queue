using Camoran.Queue.Core;
using Camoran.Queue.Util.Extensions;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client.Producer
{
    public class CamoranProducer : Client<ProducerRequest, ProducerResponse> 
    {
        public IProducerMessageBuilder ProducerMessageBuilder { get; private set; }

        private byte[] _currentBody;
        private int _sendInterval = 100;
        private Action<ProducerResponse> _sendCallback;
        private System.Timers.Timer _producerTimer = new System.Timers.Timer();
        IClient<ProducerRequest, ProducerResponse> _inner;
        private object lockObj = new object();

        public CamoranProducer(Guid clientId, ClientConfig config, IClient<ProducerRequest, ProducerResponse> inner)
            :base(clientId,config)
        {
            ProducerMessageBuilder = new ProducerMessageBuilder();
            this._inner = inner;
            this._inner.OnClientFailtoConnect=WhenProducerConnectFail;
            SetSceduleWork();
        }

 

        public override ProducerResponse SendRequest(ProducerRequest request)
        {
            return _inner.SendRequest(request);
        }


        public CamoranProducer BindTopic(string topic)
        {
            base.CurrentTopic = topic;
            return this;
        }

        public CamoranProducer SetBody(byte[] body)
        {
            this._currentBody = body;
            return this;
        }

        public CamoranProducer BindSendCallBack(Action<ProducerResponse> sendCallback)
        {
            this._sendCallback = sendCallback;
            return this;
        }

        public override void ConnectToServer()
        {
            _inner.ConnectToServer();
        }
        public override void Start()
        {
            this._producerTimer.Start();
            this._inner.Start();
        }

        public override void Stop()
        {
            this._producerTimer.Stop();
            var disconnectRequest = this.DisConnectRequest();
            this.SendRequest(disconnectRequest);
            this._inner.Stop();
        }

        public override void Close()
        {
            this._producerTimer.Close();
            this._inner.Close();
        }

        protected virtual ProducerResponse WhenProducerConnectFail(ProducerRequest request)
        {
            //log
            //var errorMsg =  string.Format( "producer id :{0} can't connect to broker now",request.SenderId);
            //byte[] errorBytes = Encoding.UTF8.GetBytes(errorMsg);
            //var response= ProducerMessageBuilder.BuildResponseMessage(request.Topic, errorBytes, request.SenderId);
            //response.Error = new Core.Message.ErrorMessage { Body= errorBytes };
            //return response;
            return null;
        }
        protected virtual void SetSceduleWork()
        {
            this._producerTimer.SetSceduleWork(_sendInterval, (e, o) =>
            {
                Util.Helper.ThreadHelper.TryLock(lockObj, () =>
                {
                    var sendMessageRequest = CreateSendRequest();
                    var response = this.SendRequest(sendMessageRequest);
                    this._sendCallback.Invoke(response);
                }, null);
            }); 
        }


        private ProducerRequest CreateSendRequest()
        {
            return ProducerMessageBuilder.BuildSendRequestMessage(base.CurrentTopic, _currentBody, this.ClientId, ProducerRequestType.send);
        }

        private ProducerRequest DisConnectRequest()
        {
            return ProducerMessageBuilder.BuildSendRequestMessage(base.CurrentTopic, _currentBody, this.ClientId, ProducerRequestType.disconnect);
        }
    }




}

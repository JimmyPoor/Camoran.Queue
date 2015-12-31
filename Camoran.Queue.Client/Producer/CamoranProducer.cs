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
        private int _sendInterval = 200;
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
            this._inner.Stop();
        }

        public override void Close()
        {
            SendDisconnRequest(null);
            this._producerTimer.Close();
            this._inner.Close();
        }

        protected virtual ProducerResponse SendDisconnRequest(ProducerRequest request)
        {
            var disconnectRequest = this.CreateRequestByRequestType(ProducerRequestType.disconnect);
            return this.SendRequest(disconnectRequest);
        }

        protected virtual ProducerResponse WhenProducerConnectFail(ProducerRequest request)
        {
            return null;
        }
        private void SetSceduleWork()
        {
            this._producerTimer.SetSceduleWork(_sendInterval, (e, o) =>
            {
                Util.Helper.ThreadHelper.TryLock(lockObj, () =>
                {
                    var sendMessageRequest = CreateRequestByRequestType(ProducerRequestType.send);
                    var response = this.SendRequest(sendMessageRequest);
                    this._sendCallback.Invoke(response);
                }, null);
            }); 
        }

        private ProducerRequest CreateRequestByRequestType(ProducerRequestType requestType)
        {
            return ProducerMessageBuilder.BuildSendRequestMessage(base.CurrentTopic, _currentBody, this.ClientId, requestType);
        }
    }




}

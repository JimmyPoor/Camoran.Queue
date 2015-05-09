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
        public string CurrentTopic { get { return _currentTopic; } }

        private string _currentTopic;
        private byte[] _currentBody;
        private int _sendInterval = 500;
        private Action<ProducerResponse> _sendCallback;
        private System.Timers.Timer _producerTimer = new System.Timers.Timer();

        private object lockObj = new object();
        public CamoranProducer(Guid clientId, HostConfig config)
            : base(clientId, config)
        {
            ProducerMessageBuilder = new ProducerMessageBuilder();
            SetSceduleWork();
        }

        public override ProducerResponse SendRequest(ProducerRequest request)
        {
            return base.SendRequest(request);
        }


        public CamoranProducer BindTopic(string topic)
        {
            this._currentTopic = topic;
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
            this.SocketClient.ConnectToServer(this.Config.Address, this.Config.Port);
        }
        public override void Start()
        {
            ConnectToServer();
            this._producerTimer.Start();

        }

        public override void Stop()
        {
            this._producerTimer.Stop();

            var disconnectRequest = this.DisConnectRequest();
            this.SendRequest(disconnectRequest);
        }

        public override void Close()
        {
         
            this._producerTimer.Close();
            //log 
        }


        private void SetSceduleWork()
        {
            this._producerTimer.SetSceduleWork(_sendInterval, (e, o) =>
            {
                lock (lockObj)
                {
                    var sendMessageRequest = CreateSendRequest();
                    var response = this.SendRequest(sendMessageRequest);
                    this._sendCallback.Invoke(response);
                }
            });
        }


        private ProducerRequest CreateSendRequest()
        {
            return ProducerMessageBuilder.BuildSendRequestMessage(_currentTopic, _currentBody, this.ClientId, ProducerRequestType.send);
        }

        private ProducerRequest DisConnectRequest() 
        {
            return ProducerMessageBuilder.BuildSendRequestMessage(_currentTopic, _currentBody, this.ClientId, ProducerRequestType.disconnect);
        }

    }
}

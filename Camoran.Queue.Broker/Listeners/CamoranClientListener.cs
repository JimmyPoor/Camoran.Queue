using Camoran.Handler;
using Camoran.Queue.Client;
using Camoran.Queue.Util.Serialize;
using Camoran.Socket;
using Camoran.Socket.Server;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;

namespace Camoran.Queue.Broker.Listeners
{


    public class CamoranClientListener<Request, Response> : IClientListener
        where Request : ClientMessage
        where Response : ClientMessage
    {
        protected SocketServer _clientServer;
        private string address;
        private int port;
        private static object lockobj = new object();
        public ConcurrentDictionary<string, Func<Request, Response>> ReceiveEvents { get; set; }

        public CamoranClientListener(string address, int port)
        {
            this.address = address;
            this.port = port;
            ReceiveEvents = new ConcurrentDictionary<string, Func<Request, Response>>();
        }

        public virtual void StartListen()
        {
            if (this._clientServer == null) { IntitalServer(); }
            _clientServer.Run();
        }
        public virtual void StopListen()
        {
            if (this._clientServer == null) return;
            _clientServer.Stop();
        }

        private void IntitalServer()
        {

            _clientServer = new SocketServer()
                .AddHandler(new ClientReceiveEventHandler(this, new ProtoBufSerializeProcessor()))
                .BindAddress(address, port);
        }

        public class ClientReceiveEventHandler : IChannelHandler
        {
            CamoranClientListener<Request, Response> _listener;
            protected ISerializeProcessor SP { get; private set; }
            public ClientReceiveEventHandler(CamoranClientListener<Request, Response> listener, ISerializeProcessor sp)
            {
                this._listener = listener;
                SP = sp;
            }

            public ChannelHanderType ChannelHandlerType
            {
                get { return ChannelHanderType.input; }
            }

            public void Handle(IChannelHandlerContext ctx)
            {
                Thread.Sleep(200);
                Request msg = default(Request);
                Response response = default(Response);

                var scc = ctx as SocketChannelContext;
                msg = SP.Deserialize<Request>(scc.Message.buffer);

                Func<Request, Response> clientRequestAction = null;
                bool hasRequestAction = _listener.ReceiveEvents.TryGetValue(msg.MessageType, out clientRequestAction);
                if (hasRequestAction)
                    response = clientRequestAction.Invoke(msg);
                scc.Message.buffer = SP.Serialize(response); // serialize response message and send it to client 
            }

            public string HandlerName
            {
                get { return "Client Receive"; }
            }
        }
    }

}

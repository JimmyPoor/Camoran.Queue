using Camoran.Queue.Client;
using Camoran.Queue.Util.Helper;
using Camoran.Queue.Util.Serialize;
using Helios.Net;
using Helios.Ops.Executors;
using Helios.Reactor;
using Helios.Reactor.Bootstrap;
using Helios.Topology;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;

namespace Camoran.Queue.Broker.Listeners
{
    public class CamoranClientListener_ByHelios<Request, Response> : IClientListener<Request, Response>
        where Request : ClientMessage
        where Response : ClientMessage
    {
        public ConcurrentDictionary<string, Func<Request, Response>> ReceiveEvents { get; set; } = new ConcurrentDictionary<string, Func<Request, Response>>();
        Server _heliosServer;
    

        public CamoranClientListener_ByHelios(string address, int port, ISerializeProcessor serializor)
            :this( address,port,serializor,false)
        {

        }

        public CamoranClientListener_ByHelios(string address, int port, ISerializeProcessor serializor,bool IsIPAddressAny)
        {
            if (port <= 0) { throw new ArgumentOutOfRangeException("port value must be larger than 0"); }
            if(!IsIPAddressAny && string.IsNullOrEmpty(address)) { throw new ArgumentNullException("address can't be null or empty"); }
            _heliosServer = new Server(address, port, this, serializor, IsIPAddressAny);
            //new ProtoBufSerializeProcessor());
        }

        public void StartListen()
        {
            _heliosServer.Start();
        }

        public void StopListen()
        {
            _heliosServer.Stop();
        }


        public class Server
        {
            string _address;
            int _port;
            IClientListener<Request, Response> _lisenter;
            IReactor _server;
            private object lockobj = new object();
            protected ISerializeProcessor SP { get; private set; }
            bool isAnyIPAddress { get; }

            public Server(string address, int port, IClientListener<Request, Response> listener, ISerializeProcessor sp,bool isIPAddressAny)
            {
                this._address = address;
                this._port = port;
                this._lisenter = listener;
                this.SP = sp;
                this.isAnyIPAddress = isIPAddressAny || string.IsNullOrEmpty(_address);

                var excutor = new TryCatchExecutor((e) =>
                {
                    //Console.Write(e);
                });
                var bootStrapper = new ServerBootstrap()
                    .Executor(excutor)
                    .SetTransport(System.Net.TransportType.Tcp)
                    .Build();
                _server = bootStrapper.NewReactor(
                 NodeBuilder.BuildNode()
                 .Host(isIPAddressAny? IPAddress.Any:IPAddress.Parse(_address))
                 .WithPort(port)
                 );

                _server.OnConnection += _server_OnConnection; ;
                _server.OnDisconnection += TempServer_OnDisconnection;
            }

            private void _server_OnConnection(INode remoteAddress, IConnection responseChannel)
            {
                responseChannel.BeginReceive(TempServer_OnReceive);
            }

            public void Start()
            {
                if (!_server.IsActive)
                    _server.Start();
            }

            public void Stop()
            {
                if (_server.IsActive)
                    _server.Stop();
            }

            private void TempServer_OnDisconnection(Helios.Exceptions.HeliosConnectionException reason, Helios.Net.IConnection closedChannel)
            {
            }


            private void TempServer_OnReceive(Helios.Net.NetworkData incomingData, IConnection connection)
            {
                ThreadHelper.TryLock(this.lockobj,
                    () => {
                            Request msg = default(Request);
                            Response response = default(Response);
                            msg = SP.Deserialize<Request>(incomingData.Buffer);

                            Func<Request, Response> clientRequestAction = null;
                            bool hasRequestAction = this._lisenter.ReceiveEvents.TryGetValue(msg.MessageType, out clientRequestAction);
                            if (hasRequestAction)
                                response = clientRequestAction.Invoke(msg);
                            var sendBuffer = SP.Serialize(response);// serialize response message and send it to client 
                            connection.Send(new Helios.Net.NetworkData
                            {
                                Buffer = sendBuffer,
                                Length = sendBuffer.Length,
                                RemoteHost = connection.RemoteHost
                            });
                    }, (error) => {
                       // TempServer_OnReceive(incomingData,connection);
                    } ); 
            }
        }
    }
}

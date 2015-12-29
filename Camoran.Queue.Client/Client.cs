using Camoran.Queue.Util.Serialize;
using Camoran.Socket.Client;
using Camoran.Queue.Util.Extensions;
using System;
using System.Threading;
using Helios.Topology;
using Helios.Net;
using Helios.Net.Bootstrap;
using System.Net;
using Helios.Exceptions;
using Camoran.Queue.Util.Helper;

namespace Camoran.Queue.Client
{
    public abstract class Client<Request, Response> : IClient<Request, Response>
        where Request : ClientMessage
        where Response : ClientMessage
    {
        public DateTime CreateDate { get; private set; }
        public DateTime StartWorkingDate { get; set; }
        public ISerializeProcessor SP { get; protected set; }
        public ClientConfig Config { get; set; }
        public ClientStatus Status { get; set; }


        public Guid ClientId
        {
            get;
            protected set;
        }

        public Func<Request, Response> OnClientFailtoConnect { get; set; }

        public Client(Guid clientId, ClientConfig config)
        {
            this.ClientId = clientId;
            this.Config = config;
            SP = new ProtoBufSerializeProcessor();
            this.CreateDate = DateTime.Now;
        }


        public abstract void ConnectToServer();

        public abstract void Start();

        public abstract void Stop();

        public abstract void Close();

        public virtual bool IsTimeout(int timeoutSeconds)
        {
            return
                //this.Status != ClientStatus.wait &&
                DateTime.Now.AddSeconds(-timeoutSeconds) > this.StartWorkingDate;
        }

        public abstract Response SendRequest(Request request);

        public virtual void SetClientStatus(ClientStatus status)
        {
            this.Status = status;
        }

    }



    public class Client_byHelios<Request, Response> : Client<Request, Response>
        where Request : ClientMessage
        where Response : ClientMessage
    {
        public INode RemoteHost;
        public IConnection Connection;
        private ManualResetEvent are = new ManualResetEvent(false);
        public Client_byHelios(Guid clientId, ClientConfig conifg) 
            : base(clientId, conifg)
        {
        }
        public override void ConnectToServer()
        {
            RemoteHost = NodeBuilder.BuildNode()
            .Host(this.Config.Address)
            .WithPort(this.Config.Port)
            .WithTransportType(System.Net.TransportType.Tcp);

            Connection = new ClientBootstrap()
                .SetTransport(System.Net.TransportType.Tcp)
                .RemoteAddress(RemoteHost)
                .OnConnect(ConnectionEstablishedCallback)
                .OnDisconnect(ConnectionTerminatedCallback)
                .Build()
                .NewConnection(RemoteHost);

            Connection.Open();
        }

        Response _response;
        public override Response SendRequest(Request request)
        {
            if (!ConnectIsOpen())
            {
                 _response = this.OnClientFailtoConnect(request);
            }
            else
            {
                are.Reset();
                EnsureConnectionIsOpen();
                byte[] buffer = SP.Serialize(request);
                _response = default(Response);
                Connection.BeginReceive((data, i) =>
                {
                    _response = SP.Deserialize<Response>(data.Buffer);
                    are.Set();
                });
                Connection.Send(new NetworkData
                {
                    Buffer = buffer,
                    Length = buffer.Length,
                    RemoteHost = RemoteHost
                });
                are.WaitOne();
            }
            return _response;
        }

        private void ReceivedDataCallback(NetworkData incomingData, IConnection responseChannel)
        {
            string.Format("Received {0} bytes from {1}", incomingData.Length,
                   incomingData.RemoteHost);
        }

        private void ConnectionEstablishedCallback(INode reason, IConnection closedChannel)
        {
            //log
        }

        private void ConnectionTerminatedCallback(HeliosConnectionException reason, IConnection closedChannel)
        {
            //log
        }

        public override void Start()
        {
            this.EnsureConnectionIsOpen();
        }

        public override void Stop()
        {
            if (Connection.IsOpen())
            {
                Connection.StopReceive();
                Connection.Close();
            }
        }

        public override void Close()
        {
            Connection.Close();
        }

        private void EnsureConnectionIsOpen()
        {
            if (Connection == null || !ConnectIsOpen()) { ConnectToServer(); }
        }

        private bool ConnectIsOpen()
        {
            return Connection.IsOpen();

        }


    }
}

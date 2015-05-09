using Camoran.Queue.Util.Serialize;
using Camoran.Socket.Client;
using Camoran.Queue.Util.Extensions;
using System;
using System.Threading;

namespace Camoran.Queue.Client
{
    public abstract class Client<Request, Response> : IClient<Request, Response>
        where Request : ClientMessage
        where Response : ClientMessage
    {
        public DateTime CreateDate { get; private set; }
        public DateTime StartWorkingDate { get; set; }
        public ISerializeProcessor SP { get; protected set; }
        public SocketClient SocketClient { get; protected set; }
        public HostConfig Config { get; set; }
        public ClientStatus Status { get; set; }

   

        private ManualResetEvent are = new ManualResetEvent(false);


        private Response responseObj;
        public Guid ClientId
        {
            get;
            protected set;
        }

        public Client(Guid clientId, HostConfig config)
        {
            this.ClientId = clientId;
            this.Config = config;
            SocketClient = new SocketClient();
            SP = new ProtoBufSerializeProcessor();
            this.CreateDate = DateTime.Now;
        }

        public abstract void ConnectToServer();

        public abstract void Start();

        public abstract void Stop();

        public abstract void Close();

        public virtual bool IsTimeout(int timeoutSeconds)
        {
            return this.Status != ClientStatus.wait
                && DateTime.Now.AddSeconds(-timeoutSeconds) > this.StartWorkingDate;
        }

        public virtual Response SendRequest(Request request)
        {
            are.Reset();
            byte[] message = SP.Serialize(request);
            responseObj = default(Response);
            SendMessage(SocketClient, message);
            SocketClient.BeginReceive((response) =>
            {
                responseObj = SP.Deserialize<Response>(response);
                are.Set();
            });
            are.WaitOne();
            return responseObj;
        }
        public virtual void SetClientStatus(ClientStatus status)
        {
            this.Status = status;
        }


        private void SendMessage<T>(SocketClient sclient, T t)
        {
            if (t == null) throw new ArgumentNullException("Instance of T can't be null");
            if (t is byte[])
            {
                sclient.BeginSend(t as byte[]);
            }
            var type = t.GetType();
            if (!type.IsComplexType())
            {
                byte[] message = System.Text.Encoding.UTF8.GetBytes(t.ToString());
                sclient.BeginSend(message);
            }

        }

    }
}

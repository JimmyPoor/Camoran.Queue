using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Client
{
    public interface IClient
    {
        Guid ClientId { get; }
        ClientStatus Status { get; set; }
        DateTime StartWorkingDate { get; set; }
        bool IsTimeout(int timeoutSeconds);
        void Start();
        void Stop();
        void Close();
    }
    public interface IClient<Request, Response> : IClient
    {
        Response SendRequest(Request request);
        void ConnectToServer();
        Func<Request, Response> OnClientFailtoConnect { get; set; }
    }

}

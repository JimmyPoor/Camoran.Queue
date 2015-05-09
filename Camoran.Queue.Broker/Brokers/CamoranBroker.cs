using Camoran.Queue.Broker.Sessions;
using Camoran.Queue.Core.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Broker.Brokers
{
    public class CamoranBroker
    {
        CamoranBrokerMachine Marchine;

       public CamoranBroker()
       {
           Marchine = new CamoranBrokerMachine();
       }

       public  void Start()
       {
           Marchine.Start();
       }
       public void Stop()
       {
           Marchine.Stop();
       }

    } 
}

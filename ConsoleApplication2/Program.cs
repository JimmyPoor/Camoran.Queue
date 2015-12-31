using Camoran.Queue.UnitTest.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            ProducerTest test = new ProducerTest();
            test.Start_Producer_Whole_Action_Mulit_Producers();
            //new ClientMixTest().Multi_Sender_Send_WhenMulit_Consumers();
            Console.Read();
        }
    }
}

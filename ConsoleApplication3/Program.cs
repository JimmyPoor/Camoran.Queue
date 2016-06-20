using Camoran.Queue.UnitTest.Client.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsumerTest test = new ConsumerTest();
            test.Start_Consume_Whole_Action_With_Diff_Topic_Test();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.UnitTest
{
    public static class TestConfig
    {
        public static string BrokerAddress = "127.0.0.1";

        public static int ProducerListenerPort = 8081;

        public static int ConsumerListenerPort = 8080;

        public static int Producer_Send_Count =1000;

        public static int producerCount = 100;

        public static int consumerCount =50;
    }
}

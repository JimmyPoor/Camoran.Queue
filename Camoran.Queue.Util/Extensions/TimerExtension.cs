using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Extensions
{
    public static class TimerExtension
    {
        public static void SetSceduleWork(this System.Timers.Timer timer,int interval,System.Timers.ElapsedEventHandler handler){
            timer.Interval = interval;
            timer.Elapsed += handler;
            timer.AutoReset = true;
        }


        public static void Delay(this System.Timers.Timer timer, int delay, System.Timers.ElapsedEventHandler handler)
        {
            timer.Elapsed += handler;
            timer.AutoReset = false;
            timer.Interval = delay;
        }
    }
}

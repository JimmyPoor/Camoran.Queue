using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Camoran.Queue.Util.Helper
{
    public static   class ThreadHelper
    {
        public static bool TryLock( object lockObj, Action action,Action<Exception> exceptonAction=null)
        {
            if (lockObj == null) throw new ArgumentException("lock obj is null");
            bool result = true;
            Monitor.Enter(lockObj);
            try
            {
                if (action != null)
                    action.Invoke();
            }
            catch(Exception e)
            {
                result = false;
                if (exceptonAction != null) {
                    exceptonAction.Invoke(e);
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
            return result;
        }
    }
}

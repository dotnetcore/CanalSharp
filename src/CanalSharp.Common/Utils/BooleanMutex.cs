using System.Threading;

namespace CanalSharp.Common.Utils
{
   public class BooleanMutex
    {
        public void Set()
        {
            Mutex mutex =new Mutex();
            mutex.ReleaseMutex();
        }
    }
}

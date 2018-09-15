using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Canal.Csharp.Core.Common.Utils
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BotHub
{
    public class UserAwaitable : INotifyCompletion, ICriticalNotifyCompletion
    {
        private DurableFunction _f;
        private bool _done = false;

        public UserAwaitable(DurableFunction f)
        {
            this._f = f;
        }

        public UserAwaitable GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted { get
            {
                return _done;
            }
        }

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            this._f.Suspend();
            this._done = true;
            Task.Run(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            this._f.Suspend();
            this._done = true;
            Task.Run(continuation);
        }
    }
}

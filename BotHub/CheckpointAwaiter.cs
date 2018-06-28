using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BotHub
{
    public class CheckpointAwaiter : ICriticalNotifyCompletion
    {
        private string _name;
        private DurableFunction _function;

        public static CheckpointAwaiter Completed = new CheckpointAwaiter("UserAwaitable.Completed", null);

        private CheckpointAwaiter()
        {
            this.IsCompleted = true;
        }

        public CheckpointAwaiter(string name, DurableFunction durableFunction)
        {
            this._name = name;
            this._function = durableFunction;
            this.IsCompleted = false;
        }

        public bool IsCompleted { get; private set; }

        public void GetResult()
        {
        }

        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);

        public void OnCompleted(Action continuation)
        {
            CheckpointState();
            
            this.IsCompleted = true;
            Task.Run(continuation);
        }

        public CheckpointAwaiter GetAwaiter()
        {
            return this;
        }

        void CheckpointState()
        {
            this._function.Suspend();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BotHub
{
    public class DurableFunctionCheckpoint : ICriticalNotifyCompletion
    {
        private string _name;
        private DurableFunction _function;

        public static DurableFunctionCheckpoint Completed = new DurableFunctionCheckpoint("UserAwaitable.Completed", null);

        private DurableFunctionCheckpoint()
        {
            this.IsCompleted = true;
        }

        public DurableFunctionCheckpoint(string name, DurableFunction durableFunction)
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

        public DurableFunctionCheckpoint GetAwaiter()
        {
            return this;
        }

        void CheckpointState()
        {
            this._function.Suspend();
        }
    }
}

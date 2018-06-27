using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class BotQueue
    {
        Queue<BotMessage> _queue = new Queue<BotMessage>();
        Queue<TaskCompletionSource<BotMessage>> _pending = new Queue<TaskCompletionSource<BotMessage>>();

        public void Enqueue(BotMessage message)
        {
            lock (_queue)
            {
                _queue.Enqueue(message);

                ResolvePendingTasks();
            }
        }

        void ResolvePendingTasks()
        {
            if(_queue.Count > 0 && _pending.Count > 0)
            {
                if( _queue.TryDequeue(out var message) &&
                    _pending.TryDequeue(out var task))
                {
                    task.SetResult(message);
                }
            }
        }

        public Task<BotMessage> DequeueAsync()
        {
            lock(_queue)
            {
                if(_queue.TryDequeue(out var message))
                {
                    return Task.FromResult(message);
                }

                return CreatePendingDequeue();
            }
        }  
        
        Task<BotMessage> CreatePendingDequeue()
        {
            var tsc = new TaskCompletionSource<BotMessage>();
            _pending.Enqueue(tsc);

            return tsc.Task;
        }
    }
}

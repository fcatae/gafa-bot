using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowServer
    {
        readonly WorkflowQueue _queue;

        public WorkflowServer(WorkflowQueue queue)
        {
            _queue = queue;
        }

        public void DoEventLoop()
        {
            var message = _queue.Dequeue();

            while(message != null)
            {
                ProcessMessage(message);
                message = _queue.Dequeue();
            }
        }

        protected virtual void ProcessMessage(WorkflowMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Console.WriteLine(message.Module + ":" + message.Method);
        }
    }
}

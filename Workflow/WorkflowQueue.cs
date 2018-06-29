using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowQueue : IWorkflowQueue
    {
        Queue<WorkflowMessage> _queue = new Queue<WorkflowMessage>();

        public void Enqueue(WorkflowMessage message)
        {
            _queue.Enqueue(message);
        }

        public WorkflowMessage Dequeue()
        {
            _queue.TryDequeue(out var message);
            return message;
        }
    }
}

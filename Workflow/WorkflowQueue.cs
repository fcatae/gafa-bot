using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowQueue : IWorkflowQueue
    {
        Queue<string> _queue = new Queue<string>();
        int _idGenerator = 0;

        public void Enqueue(WorkflowMessage message)
        {
            _queue.Enqueue(message.GetContent());
        }

        public WorkflowMessage Dequeue()
        {
            if(_queue.TryDequeue(out string content))
            {
                return WorkflowMessage.CreateFrom(GetId(), content, null);
            }

            return null;
        }

        public void Complete(WorkflowMessage message)
        {
            // ignore
        }

        string GetId()
        {
            return "TEST" + (_idGenerator++).ToString();
        }
    }
}

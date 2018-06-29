using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowClient
    {
        readonly WorkflowQueue _queue;

        public WorkflowClient(WorkflowQueue queue)
        {
            _queue = queue;
        }

        public void Start(string module, string method, object parameter)
        {
            var message = new WorkflowMessage(module, method, parameter);
            _queue.Enqueue(message);
        }
    }
}

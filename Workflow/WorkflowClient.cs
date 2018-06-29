using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowClient
    {
        readonly IWorkflowQueue _queue;

        public WorkflowClient(IWorkflowQueue queue)
        {
            _queue = queue;
        }

        public void Start(string module, string method, object parameter)
        {
            var message = WorkflowMessage.CreateCall(module, method, parameter);
            _queue.Enqueue(message);
        }
    }
}

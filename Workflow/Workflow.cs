using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class Workflow
    {
        IWorkflowQueue _queue;

        public Workflow() : this(new WorkflowQueue())
        {
        }

        public Workflow(IWorkflowQueue queue)
        {
            _queue = queue;
        }

        public WorkflowClient GetClient()
        {
            return new WorkflowClient(_queue);
        }

        public WorkflowServer GetServer()
        {
            return new WorkflowServer(_queue);
        }
    }
}

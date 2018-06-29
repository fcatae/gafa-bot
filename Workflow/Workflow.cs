using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class Workflow
    {
        WorkflowQueue _queue = new WorkflowQueue();

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

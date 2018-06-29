using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class Workflow
    {
        public WorkflowClient GetClient()
        {
            return new WorkflowClient();
        }

        public WorkflowServer GetServer()
        {
            return new WorkflowServer();
        }
    }
}

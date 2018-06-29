using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowClassAttribute : Attribute
    {
        public readonly int Timeout;

        public WorkflowClassAttribute(int timeout = 5)
        {
            this.Timeout = timeout;
        }
    }
}

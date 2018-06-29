using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowMessage
    {
        public readonly string Module;
        public readonly string Method;
        public readonly object Parameter;

        public WorkflowMessage(string module, string method, object parameter)
        {
            this.Module = module;
            this.Method = method;
            this.Parameter = parameter;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow
{
    class WorkflowInterruptionException : Exception
    {
        RuntimeContext.WorkflowState _state;

        public WorkflowInterruptionException(RuntimeContext.WorkflowState state)
        {
            _state = state;
        }

        public RuntimeContext.WorkflowState WorkflowState => _state;
    }
}

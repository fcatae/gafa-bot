﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    interface IWorkflowQueue
    {
        void Enqueue(WorkflowMessage message);
        WorkflowMessage Dequeue();
        void UpdateTimeout(WorkflowMessage message, int timeout);
        void Complete(WorkflowMessage message);
    }
}

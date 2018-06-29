using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Workflow
{
    class WorkflowServer
    {
        readonly IWorkflowQueue _queue;

        public WorkflowServer(IWorkflowQueue queue)
        {
            _queue = queue;
        }

        public void DoEventLoop()
        {
            var message = _queue.Dequeue();

            while(message != null)
            {
                ProcessMessage(message);
                _queue.Complete(message);

                message = _queue.Dequeue();
            }
        }

        protected virtual void ProcessMessage(WorkflowMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            RunLocal(message.Module, message.Method, message.Parameter);
        }

        void RunLocal(string typeName, string methodName, object parameter)
        {
            var type = Type.GetType(typeName);
            var method = type.GetMethod(methodName);
            var instance = Activator.CreateInstance(type);

            CheckTypeSecurity(type);

            method.Invoke(instance, new object[] { parameter });
        }

        void CheckTypeSecurity(Type type)
        {
            if(type.GetCustomAttribute<WorkflowClassAttribute>() == null)
            {
                throw new InvalidOperationException($"Class {type.FullName} does not have WorkflowClassAttribute");
            }
        }
    }
}

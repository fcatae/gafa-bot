using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Workflow
{
    class WorkflowServer
    {
        readonly WorkflowQueue _queue;

        public WorkflowServer(WorkflowQueue queue)
        {
            _queue = queue;
        }

        public void DoEventLoop()
        {
            var message = _queue.Dequeue();

            while(message != null)
            {
                ProcessMessage(message);
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

            method.Invoke(instance, new object[] { parameter });
        }
    }
}

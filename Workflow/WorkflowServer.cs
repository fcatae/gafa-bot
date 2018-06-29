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
                try
                {
                    ProcessMessage(message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                message = _queue.Dequeue();
            }
        }

        protected virtual void ProcessMessage(WorkflowMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            int timeout = GetModuleTimeout(message.Module);

            message.UpdateTimeout(timeout);

            RunLocal(message.Module, message.Method, message.Parameter);

            message.Complete();
        }

        int GetModuleTimeout(string typeName)
        {
            var type = Type.GetType(typeName);
            int timeout = type.GetCustomAttribute<WorkflowClassAttribute>().Timeout;

            return timeout;
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

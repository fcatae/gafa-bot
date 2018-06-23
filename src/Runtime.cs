using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class Runtime
    {
        public static async Task<string> RunAsync(string objectName, string methodName, params object[] parameters)
        {
            try
            {
                await RunObjectMethodAsync(objectName, methodName, parameters);
            }
            catch (WorkflowInterruptionException ex)
            {
                return ex.WorkflowState;
            }

            return null;
        }

        public static async Task<string> RunAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (WorkflowInterruptionException ex)
            {
                return ex.WorkflowState;
            }

            return null;
        }

        public static Task RunObjectMethodAsync(string objectName, string methodName, params object[] parameters)
        {
            Type type = Type.GetType(objectName);
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(methodName);

            return methodInfo.Invoke(instance, parameters) as Task;
        }
    }
}

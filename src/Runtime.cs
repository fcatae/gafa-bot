using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class RuntimeFunctionCall
    {
        readonly string _objectName;
        readonly string _methodName;
        readonly object[] _parameters;

        public RuntimeFunctionCall(string objectName, string methodName, object[] parameters)
        {
            this._objectName = objectName;
            this._methodName = methodName;
            this._parameters = parameters;
        }

        public object Execute()
        {
            Type type = Type.GetType(_objectName);
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(_methodName);

            return methodInfo.Invoke(instance, _parameters);
        }

        public object Execute(params object[] parameters)
        {
            Type type = Type.GetType(_objectName);
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(_methodName);

            return methodInfo.Invoke(instance, parameters);
        }
    }

    class Runtime
    {
        public static RuntimeContext Start(string objectName, string methodName, params object[] parameters)
        {
            var funcCall = new RuntimeFunctionCall(objectName, methodName, parameters);

            var context = new RuntimeContext(funcCall);
            var task = StartAsync(context, objectName, methodName, parameters);

            return context;
        }

        public static async Task StartAsync(RuntimeContext context, string objectName, string methodName, params object[] parameters)
        {
            try
            {
                await RunObjectMethodAsync(objectName, methodName, parameters).ConfigureAwait(false);

                context.SetState(null);
                context.SetResult(true);
            }
            catch (WorkflowInterruptionException ex)
            {
                context.SetState(ex.WorkflowState);
                context.SetPauseState();
            }
        }

        static async Task<RuntimeContext.WorkflowState> RunAsync(Func<Task> action)
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

            return (Task)methodInfo.Invoke(instance, parameters);
        }
    }
}

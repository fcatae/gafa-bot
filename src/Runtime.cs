using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class RuntimeContext
    {
        private TaskCompletionSource<object> _tsc = new TaskCompletionSource<object>();

        string _state = null;
        bool _hasFinished = false;
        
        public void SetState(string state)
        {
            this._state = state;
        }

        public string GetState()
        {
            return this._state;
        }

        public void SetResult(object result)
        {
            _tsc.SetResult(result);
        }

        public Task GetTask()
        {
            return this._tsc.Task;
        }

        public void Continue()
        {
        }

        public bool HasFinished => _hasFinished;
    }

    class Runtime
    {
        public static RuntimeContext Start(string objectName, string methodName, params object[] parameters)
        {
            var context = new RuntimeContext();
            var task = StartAsync(context, objectName, methodName, parameters).ConfigureAwait(false);

            return context;
        }

        public static async Task StartAsync(RuntimeContext context, string objectName, string methodName, params object[] parameters)
        {
            try
            {
                await RunObjectMethodAsync(objectName, methodName, parameters);

                context.SetState(null);
                context.SetResult(true);
            }
            catch (WorkflowInterruptionException ex)
            {
                context.SetState(ex.WorkflowState);
            }
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

            return (Task)methodInfo.Invoke(instance, parameters);
        }
    }
}

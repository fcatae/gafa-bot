using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BotHub
{
    public class TaskInspector
    {
        IAsyncStateMachine _state;

        public void Scan()
        {
            // list types
            var assembly = Assembly.GetCallingAssembly();
            var types = assembly.GetTypes();
        }

        public void Run<T>(string methodName)
        {
            var objType = typeof(T);
            var method = objType.GetMethod(methodName);
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
            var stateType = attrib.StateMachineType;

            // create type
            var stateObj = Activator.CreateInstance(stateType);
            var stateMachine = (IAsyncStateMachine)stateObj;

            // create the task builder
            var b = AsyncTaskMethodBuilder.Create();
            var taskField = stateType.GetField("<>t__builder");
            taskField.SetValue(stateMachine, b);

            var stateField = stateType.GetField("<>1__state");
            stateField.SetValue(stateMachine, -1);

            _state = stateMachine;

            //b.SetStateMachine(stateMachine);
            b.Start(ref stateMachine);

            //while(true)
            //{
            //    stateMachine.MoveNext();
            //}

        }
    }
}

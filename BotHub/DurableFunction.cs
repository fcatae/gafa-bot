using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BotHub
{
    public class DurableFunction
    {
        class Definition
        {
            readonly Type _stateMachineType;
            readonly FieldInfo _fieldBuilder;
            readonly FieldInfo _fieldThis;
            readonly FieldInfo _fieldState;
            readonly FieldInfo[] _fieldLocals;
            readonly FieldInfo[] _allFields;
            readonly object _thisPointer;

            public Definition(Type objType, string methodName, object thisPointer)
            {
                // Init state machine type
                var method = objType.GetMethod(methodName);
                var attrib = method.GetCustomAttribute<AsyncStateMachineAttribute>();
                var stateType = attrib.StateMachineType;

                _stateMachineType = stateType;

                // Retrieve the fields
                _fieldBuilder = stateType.GetField("<>t__builder");
                _fieldState = stateType.GetField("<>1__state");
                _fieldThis = stateType.GetField("<>4__this");

                // All fields
                _fieldLocals = stateType.GetRuntimeFields().Where(SelectOnlyNamedVariables).ToArray();
                _allFields = stateType.GetRuntimeFields().ToArray();

                _thisPointer = thisPointer;
            }
            
            public IAsyncStateMachine CreateStateMachine()
            {
                var stateObj = Activator.CreateInstance(_stateMachineType);
                var stateMachine = (IAsyncStateMachine)stateObj;

                return stateMachine;
            }

            public Task RunAsync(IAsyncStateMachine stateMachine)
            {
                var builder = AsyncTaskMethodBuilder.Create();

                _fieldBuilder.SetValue(stateMachine, builder);
                _fieldState.SetValue(stateMachine, -1);
                _fieldThis.SetValue(stateMachine, _thisPointer);

                builder.Start(ref stateMachine);

                return builder.Task;
            }

            public Task RunStepAsync(IAsyncStateMachine stateMachine, int step)
            {
                var builder = AsyncTaskMethodBuilder.Create();

                _fieldBuilder.SetValue(stateMachine, builder);
                _fieldState.SetValue(stateMachine, step);
                _fieldThis.SetValue(stateMachine, _thisPointer);

                builder.Start(ref stateMachine);

                return builder.Task;
            }

            bool SelectOnlyNamedVariables(FieldInfo fld)
            {
                return (fld.Name.Length > 2 && fld.Name[0] == '<' && fld.Name[1] != '>');
            }

            public int GetState(IAsyncStateMachine stateMachine)
            {
                return (int)_fieldState.GetValue(stateMachine);
            }

            public object GetLocalVariables(IAsyncStateMachine stateMachine)
            {
                return _fieldLocals.Select(fld => fld.GetValue(stateMachine)).ToArray();
            }

            public object GetAllFields(IAsyncStateMachine stateMachine)
            {
                return _allFields.Select(fld => fld.GetValue(stateMachine)).ToArray();
            }

            public void RestoreFields(IAsyncStateMachine stateMachine)
            {
                var fs =_allFields
                    //.Where(fld => fld.Name.StartsWith("<>u__"))
                    .Where(fld => fld.FieldType == typeof(System.Runtime.CompilerServices.TaskAwaiter))
                    .Select(fld => {
                        var t = Task.CompletedTask.GetAwaiter();
                        fld.SetValue(stateMachine, t); return t;
                    })
                    .ToArray();
            }
            
        }

        Definition _definition;
        IAsyncStateMachine _state;

        DurableFunction(Definition definition, IAsyncStateMachine stateMachine)
        {
            _definition = definition;
            _state = stateMachine;
        }

        public static DurableFunction Create(Type objType, string methodName, object thisPointer)
        {
            var functionDefinition = new Definition(objType, methodName, thisPointer);
            var state = functionDefinition.CreateStateMachine();

            return new DurableFunction(functionDefinition, state);
        }

        public Task StartAsync()
        {
            _definition.RestoreFields(_state);

            var task = _definition.RunStepAsync(_state, -1);

            return task;
        }
        
        void RestoreTaskAwaiter(IAsyncStateMachine state)
        {
            _definition.RestoreFields(_state);
        }

        public void Suspend()
        {
            var stt0 = _definition.GetState(_state);
            var vars = _definition.GetLocalVariables(_state);
            var al99 = _definition.GetAllFields(_state);
        }

        public void Resume()
        {
            var stt0 = _definition.GetState(_state);
            var vars = _definition.GetLocalVariables(_state);
            var al99 = _definition.GetAllFields(_state);
        }
    }
}

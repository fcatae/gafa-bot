﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

// TODO:
// (3) Support Task.WhenAll/Task.WhenAny. This will involve digging down into tasks, and will
//     only work in a single-threaded execution context.
// (4) Support ConfigureAwait(). Needs a bit more state stored in the AsyncMethodState class.
// (6) VB support in reflection


public static class Checkpoint
{
    public enum Disposition
    {
        Completed,
        Deferred
    }

    public static RunWithCheckpointingAwaitable RunWithCheckpointing(this Task task, string fn)
    {
        return new RunWithCheckpointingAwaitable { task = task, fn = fn };
        // TODO: make sure that we're already awaiting the RunWithCheckpointingAwaiter before
        // Checkpoint.Save() does its walk up the async callstack
    }

    public static Task ResumeFrom(string fn)
    {
        var tt = Deserialize(File.ReadAllText(fn));
        File.WriteAllText(fn, Serialize(tt.Item1, tt.Item2 + 1));
        var sm = ReconstructStateMachine(tt.Item1);
        sm.LeafCheckpointSaveAwaiter._result = tt.Item2 + 1;
        sm.LeafActionToStartWork.Invoke();
        return sm.Task;
    }

    struct ReadStateMachineResult
    {
        public IAsyncStateMachine StateMachine; // is a boxed state machine whose builder is wired up right
        public Task Task; // is the task from that boxed copy of the state machine
        public object AwaiterForAwaitingThisStateMachine; // (might be a struct that has to be copied in place)
        //
        public Action LeafActionToStartWork;
        public CheckpointSaveAwaiter LeafCheckpointSaveAwaiter;
    }


    private static ReadStateMachineResult ReconstructStateMachine(AsyncMethodState state)
    {
        var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        string typename = Assembly.CreateQualifiedName(state.StateMachineAssemblyName, state.StateMachineTypeName);

        // sm = new StateMachineType();
        var sm = Activator.CreateInstance(Type.GetType(typename)) as IAsyncStateMachine;

        // sm.builder = BuilderType.Create()
        var builderField = sm.BuilderField();
        builderField.SetValue(sm, builderField.FieldType.GetMethod("Create").Invoke(null, new object[] { }));

        // sm.builder.SetStateMachine(sm)
        Expression.Lambda<Action>(Expression.Call(Expression.Field(Expression.Constant(sm), builderField), builderField.FieldType.GetMethod("SetStateMachine"), Expression.Constant(sm))).Compile().Invoke();

        // sm.state = i
        sm.GetType().GetField(state.StateNumber.MemberName).SetValue(sm, state.StateNumber.Value);

        // sm.<local> = <local_value>
        foreach (var local in state.Locals) sm.GetType().GetField(local.MemberName, bf).SetValue(sm, local.Value);

        ReadStateMachineResult awaited;
        if (state.CurrentAwaiter.Value == null)
        {
            // sm.awaiter = new CheckpointSaveAwaiter()
            var awaiter = new CheckpointSaveAwaiter() { _result = 0 };
            awaited = new ReadStateMachineResult() { AwaiterForAwaitingThisStateMachine = awaiter, LeafCheckpointSaveAwaiter = awaiter };
            sm.GetType().GetField(state.CurrentAwaiter.MemberName, bf).SetValue(sm, awaiter);
        }
        else
        {
            // sm.awaiter = <nested state machine's awaiter>
            awaited = ReconstructStateMachine(state.CurrentAwaiter.Value);
            var awaiter = awaited.AwaiterForAwaitingThisStateMachine;
            sm.GetType().GetField(state.CurrentAwaiter.MemberName, bf).SetValue(sm, awaiter);
        }

        // sm.builder.AwaitOnCompleted(ref child_awaiter, ref sm);
        var varSm = Expression.Variable(sm.GetType(), "sm");
        var varAwaiter = Expression.Variable(awaited.AwaiterForAwaitingThisStateMachine.GetType(), "awaiter");
        var expression = Expression.Lambda<Action>(Expression.Block(
            new[] { varSm, varAwaiter },
            Expression.Assign(varSm, Expression.Constant(sm)),
            Expression.Assign(varAwaiter, Expression.Constant(awaited.AwaiterForAwaitingThisStateMachine)),
            Expression.Call(
                Expression.Field(varSm, builderField),
                builderField.FieldType.GetMethod("AwaitOnCompleted").MakeGenericMethod(awaited.AwaiterForAwaitingThisStateMachine.GetType(), sm.GetType()),
                new Expression[] { varAwaiter, varSm })));
        var lambda = expression.Compile();
        if (state.CurrentAwaiter.Value == null) awaited.LeafActionToStartWork = lambda;
        else lambda();

        // task = sm.Builder.Task
        var task = Expression.Lambda<Func<object>>(Expression.Property(Expression.Field(Expression.Constant(sm), builderField), builderField.FieldType.GetProperty("Task"))).Compile().Invoke();

        // task.GetAwaiter();
        var taskAwaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, new object[] { });


        return new ReadStateMachineResult
        {
            StateMachine = sm,
            Task = task as Task,
            AwaiterForAwaitingThisStateMachine = taskAwaiter,
            LeafActionToStartWork = awaited.LeafActionToStartWork,
            LeafCheckpointSaveAwaiter = awaited.LeafCheckpointSaveAwaiter
        };

    }



    public class DeferRemainderException : Exception
    {
        public DeferRemainderException(TimeSpan t) { }
        public DeferRemainderException() { }
    }

    public static CheckpointSaveAwaitable Save() => new CheckpointSaveAwaitable();

    public class RunWithCheckpointingAwaitable
    {
        public Task task;
        public string fn;
        public RunWithCheckpointingAwaiter GetAwaiter() => new RunWithCheckpointingAwaiter { awaiter = task.GetAwaiter(), fn = fn };
    }

    public class RunWithCheckpointingAwaiter : INotifyCompletion
    {
        public TaskAwaiter awaiter;
        public string fn;
        public Action continuation;
        public bool IsCompleted => awaiter.IsCompleted;
        public Disposition GetResult()
        {
            try
            {
                awaiter.GetResult();
                return Disposition.Completed;
            }
            catch (DeferRemainderException)
            {
                return Disposition.Deferred;
            }
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
            awaiter.OnCompleted(OnCompletedRunner);
        }
        public void OnCompletedRunner()
        {
            var deleteFile = true;
            try
            {
                awaiter.GetResult();
            }
            catch (DeferRemainderException)
            {
                deleteFile = false;
            }
            catch (Exception)
            {
            }
            if (deleteFile && File.Exists(fn)) File.Delete(fn);
            continuation();
        }
    }


    public class CheckpointSaveAwaitable
    {
        public CheckpointSaveAwaiter GetAwaiter() => new CheckpointSaveAwaiter() { _result = 0 };
    }

    public class CheckpointSaveAwaiter : INotifyCompletion
    {
        public int _result;
        public bool IsCompleted => false;
        public int GetResult() => _result;
        public void OnCompleted(Action continuation)
        {
            if (_result > 0)
            {
                continuation();
                return;
            }

            var asyncMethod = TryGetStateMachineForDebugger(continuation).Target as IAsyncStateMachine;

            AsyncMethodState state = null;

            while (true)
            {
                var state1 = new AsyncMethodState();
                state1.AsyncMethodName = asyncMethod.Name();
                state1.StateMachineAssemblyName = asyncMethod.GetType().Assembly.FullName;
                state1.StateMachineTypeName = asyncMethod.GetType().FullName;
                state1.StateNumber = asyncMethod.GetCurrentStateNumber();
                state1.CurrentAwaiter = CreateMember(asyncMethod.GetCurrentAwaiter().MemberName, state);
                state1.Locals = asyncMethod.GetLocalsAndParameters().ToList();
                state = state1;

                var parent = asyncMethod.GetAsyncMethodThatsAwaitingThisOne();
                if (parent == null) throw new NotSupportedException($"Can't figure out which async method is awaiting {asyncMethod.Name()}");
                var parentAwaiter = parent.GetCurrentAwaiter();
                if (parentAwaiter == null) throw new NotSupportedException($"Async method {parent.Name()} has awaiter types that we don't know how to checkpoint");
                var name = parentAwaiter.Value.GetType().ToString();
                if (parentAwaiter.Value is RunWithCheckpointingAwaiter)
                {
                    var saver = parentAwaiter.Value as RunWithCheckpointingAwaiter;
                    File.WriteAllText(saver.fn, Serialize(state, 0));
                    break;
                }
                else if (parentAwaiter.Value.GetType().ToString().StartsWith("System.Runtime.CompilerServices.TaskAwaiter"))
                {
                    asyncMethod = parent;
                }
                else
                {
                    throw new NotSupportedException($"Async method {parent.Name()} is awaiting a {parentAwaiter.Value.GetType().ToString()}, but we only know how to checkpoint awaits on normal Tasks");
                }
            }

            continuation();

        }

    }


    class AsyncMethodState
    {
        public string AsyncMethodName;
        public string StateMachineAssemblyName;
        public string StateMachineTypeName;
        public Member<int> StateNumber;
        public Member<AsyncMethodState> CurrentAwaiter;
        public List<Member<object>> Locals;
    }

    public static Member<T> CreateMember<T>(string name, T value) => new Member<T> { MemberName = name, Value = value };

    public class Member<T>
    {
        public string MemberName;
        public T Value;

        public override string ToString() => $"{MemberName} = {Value}";
    }

    private static string Serialize(AsyncMethodState state, int resumeCount)
    {
        var json = new JObject();
        json["resumeCount"] = resumeCount;
        json["state"] = SerializeInner(state);
        return json.ToString();
    }

    private static JObject SerializeInner(AsyncMethodState state)
    {
        if (state == null) return null;

        var json = new JObject();
        json["asyncMethodName"] = state.AsyncMethodName;
        json["stateMachineAssemblyName"] = state.StateMachineAssemblyName;
        json["stateMachineTypeName"] = state.StateMachineTypeName;
        json["stateNumber"] = new JObject {
            { "memberName", state.StateNumber.MemberName },
            { "value", state.StateNumber.Value } };
        json["currentAwaiter"] = new JObject {
            { "memberName", state.CurrentAwaiter.MemberName},
            { "value", SerializeInner(state.CurrentAwaiter.Value) } };

        var locals = new JArray();
        json["locals"] = locals;
        foreach (var local in state.Locals)
        {
            if (local.Value == null) locals.Add(new JObject {
                { "memberName", local.MemberName } });
            else locals.Add(new JObject {
                { "memberName", local.MemberName},
                { "assembly", local.Value.GetType().Assembly.FullName },
                { "type", local.Value.GetType().FullName },
                { "value", JToken.FromObject(local.Value) } });
        }

        return json;
    }


    private static Tuple<AsyncMethodState, int> Deserialize(string s)
    {
        var json = JObject.Parse(s);
        var resumeCount = json["resumeCount"].Value<int>();
        var state = DeserializeInner(json["state"]);
        return Tuple.Create(state, resumeCount);
    }

    private static AsyncMethodState DeserializeInner(JToken json)
    {
        if (json == null || !json.Any()) return null;
        var state = new AsyncMethodState();
        state.AsyncMethodName = json["asyncMethodName"].Value<string>();
        state.StateMachineAssemblyName = json["stateMachineAssemblyName"].Value<string>();
        state.StateMachineTypeName = json["stateMachineTypeName"].Value<string>();
        state.StateNumber = CreateMember(json["stateNumber"]["memberName"].Value<string>(), json["stateNumber"]["value"].Value<int>());
        state.CurrentAwaiter = CreateMember(json["currentAwaiter"]["memberName"].Value<string>(), DeserializeInner(json["currentAwaiter"]["value"]));
        state.Locals = new List<Member<object>>();
        foreach (var local in json["locals"])
        {
            var value = local["value"] == null
                        ? null
                        : local["value"].ToObject(Assembly.Load(local["assembly"].Value<string>()).GetType(local["type"].Value<string>()));
            state.Locals.Add(CreateMember(local["memberName"].Value<string>(), value));
        }
        return state;
    }


    private static FieldInfo BuilderField(this IAsyncStateMachine sm) => sm.GetType().GetField("<>t__builder") ?? sm.GetType().GetField("$Builder");

    public static Task Task(this IAsyncStateMachine sm)
    {
        // sm.Task
        var task = Expression.Lambda<Func<object>>(
            Expression.Property(
                Expression.Field(Expression.Constant(sm), sm.BuilderField()),
                sm.BuilderField().FieldType.GetProperty("Task"))).Compile().Invoke();
        return task as Task;
    }

    static IAsyncStateMachine GetAsyncMethodThatsAwaitingThisOne(this IAsyncStateMachine sm)
    {
        var task = sm.Task();
        if (task == null) return null;

        var continuationDelegates = GetDelegatesFromContinuationObject(task);
        var continuationDelegate = (continuationDelegates?.Length == 1 ? continuationDelegates[0] as Action : null);
        if (continuationDelegate == null) return null;

        var stateMachine = TryGetStateMachineForDebugger(continuationDelegate).Target;
        if (stateMachine is RunWithCheckpointingAwaiter)
        {
            continuationDelegate = (stateMachine as RunWithCheckpointingAwaiter).continuation;
            stateMachine = TryGetStateMachineForDebugger(continuationDelegate).Target;
        }

        var builderField = stateMachine?.GetType().GetField("<>t__builder") ?? stateMachine?.GetType().GetField("$Builder");
        if (builderField == null) return null;
        return stateMachine as IAsyncStateMachine;
    }

    static string Name(this IAsyncStateMachine sm)
    {
        var t = sm.GetType();
        var s = t.Name;
        s = s.Substring(1, s.LastIndexOf(">") - 1);
        while (true)
        {
            t = t.DeclaringType;
            if (t != null) s = t.Name + "." + s;
            else return s + $" [task id {sm.Task()?.Id}]";
        }
    }


    public static IEnumerable<Member<object>> GetLocalsAndParameters(this IAsyncStateMachine sm)
    {
        var fieldInfos = sm.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            if (fieldInfo.Name == "<>t__builder") continue; // builder
            if (fieldInfo.Name.StartsWith("<>u__")) continue; // awaiters
            if (fieldInfo.Name == "<>1__state") continue; // state
            // all other fields are locals and parameters
            var value = fieldInfo.GetValue(sm);
            yield return CreateMember(fieldInfo.Name, value);
        }
    }

    public static Member<int> GetCurrentStateNumber(this IAsyncStateMachine sm)
    {
        var fieldInfo = sm.GetType().GetField("<>1__state");
        var value = (int)fieldInfo.GetValue(sm);
        return CreateMember(fieldInfo.Name, value);
    }

    public static Member<object> GetCurrentAwaiter(this IAsyncStateMachine sm)
    {
        var candidateAwaiters = new List<Member<object>>();

        var fieldInfos = sm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            if (!fieldInfo.Name.StartsWith("<>u__")) continue;
            var awaiter = fieldInfo.GetValue(sm); if (awaiter == null) continue;

            // typical awaiters have a field named "m_task" of type Task
            var taskFieldInfo = awaiter.GetType().GetField("m_task", BindingFlags.NonPublic | BindingFlags.Instance);
            if (taskFieldInfo != null && typeof(Task).IsAssignableFrom(taskFieldInfo.FieldType))
            {
                var m_task = taskFieldInfo.GetValue(awaiter);
                if (m_task == null) continue;
            }

            candidateAwaiters.Add(CreateMember(fieldInfo.Name, awaiter));
        }

        if (candidateAwaiters.Count != 1)
        {
            throw new NotSupportedException($"Async method {sm.Name()} has awaiters that are too complicated to process");
        }
        return candidateAwaiters[0];
    }


    // Here I'm exposing some internal methods from mscorlib that I need to walk the async callstack
    private static Type _type_AsyncMethodBuilderCore = typeof(AsyncTaskMethodBuilder).Assembly.GetType("System.Runtime.CompilerServices.AsyncMethodBuilderCore");
    private static MethodInfo _mi_TryGetStateMachineForDebugger = _type_AsyncMethodBuilderCore.GetMethod("TryGetStateMachineForDebugger", BindingFlags.NonPublic | BindingFlags.Static);
    private static MethodInfo _mi_TryGetContinuationAction = _type_AsyncMethodBuilderCore.GetMethod("TryGetContinuationTask", BindingFlags.NonPublic | BindingFlags.Static);
    private static MethodInfo _mi_GetDelegatesFromContinuationObject = typeof(Task).GetMethod("GetDelegatesFromContinuationObject", BindingFlags.NonPublic | BindingFlags.Static);

    /// <summary>
    /// Retrieves the async state machine's MoveNext method
    /// </summary>
    private static Action TryGetStateMachineForDebugger(Action action) => _mi_TryGetStateMachineForDebugger.Invoke(null, new object[] { action }) as Action;

    /// <summary>
    /// Given an action (e.g. one of the delegates that will be executed upon task completion),
    /// see if it is a contiunation wrapper and has a Task associated with it.  If so return it; null otherwise.
    /// </summary>
    private static Task TryGetContinuationTask(Action action) => _mi_TryGetContinuationAction.Invoke(null, new object[] { action }) as Task;

    /// <summary>
    /// Given a task, finds all the delegates that will be executed when the task is complete
    /// </summary>
    private static Delegate[] GetDelegatesFromContinuationObject(object continuationObject) => _mi_GetDelegatesFromContinuationObject.Invoke(null, new object[] { continuationObject }) as Delegate[];


}
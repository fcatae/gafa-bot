using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace TaskFlow
{
    abstract class Workflow
    {
        WorkspaceScopeList _scopes = new WorkspaceScopeList();

        protected Task<T> ScopeAsync<T>(string name)
            where T: ScopeVariables, new()
        {
            var scope = new T();
            _scopes.Add(new WorkspaceScope { Name = name, TypeName = typeof(T).FullName, Variables = scope });

            return Task.FromResult(scope);
        }

        protected void Code(string name, Action action)
        {
            var initialScope = _scopes.Clone();

            try
            {
                action();
            }
            catch(InjectFailureException)
            {
                var state = new RuntimeContext.WorkflowState(name, initialScope);
                throw new WorkflowInterruptionException(state);
            }            
        }
    }

    class WorkspaceScopeList
    {
        List<WorkspaceScope> _scopes = new List<WorkspaceScope>();
        
        public void Add(WorkspaceScope scope)
        {
            _scopes.Add(scope);
        }

        public static string Serialize(WorkspaceScopeList workspaceScopeList)
        {
            return JsonConvert.SerializeObject(workspaceScopeList._scopes);
        }

        public static WorkspaceScopeList Deserialize(string text)
        {
            var list = JsonConvert.DeserializeObject<List<WorkspaceScopeGeneric>>(text);

            var scopeList = new WorkspaceScopeList();

            foreach (var line in list)
            {
                var variableType = Type.GetType(line.TypeName);
                var variables = line.Variables.ToObject(variableType);

                WorkspaceScope scope = new WorkspaceScope()
                {
                    Name = line.Name,
                    TypeName = line.TypeName,
                    Variables = (ScopeVariables)variables
                };

                scopeList.Add(scope);
            }
            return scopeList;
        }

        public WorkspaceScopeList Clone()
        {
            var clone = new WorkspaceScopeList();

            foreach(var scope in this._scopes)
            {
                clone.Add(scope.Clone());
            }

            return clone;
        }

        class WorkspaceScopeGeneric
        {
            public string Name { get; set; }
            public string TypeName { get; set; }
            public JObject Variables { get; set; }
        }
    }

    class WorkspaceScope
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public ScopeVariables Variables { get; set; }

        public WorkspaceScope Clone()
        {
            return new WorkspaceScope
            {
                Name = Name,
                TypeName = TypeName,
                Variables = Variables.Clone()
            };
        }
    }

    abstract class ScopeVariables : IDisposable
    {
        Dictionary<Type,object> _instances = new Dictionary<Type, object>();
        
        public bool IsActive()
        {
            return (_instances != null);
        }

        public T Use<T>()
            where T: new()
        {
            if (_instances == null)
                throw new InvalidOperationException("Object already disposed");

            Type key = typeof(T);

            _instances.TryGetValue(key, out object instance);

            if (instance == null)
            {
                instance = new T();
                _instances.Add(key, instance);
            }
            
            return (T)instance;
        }

        public ScopeVariables Clone()
        {
            return (ScopeVariables)this.MemberwiseClone();
        }

        public void Dispose()
        {
            foreach(var instance in _instances.Values)
            {
                var disposable = instance as IDisposable;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
            }
            _instances = null;
        }
    }
}

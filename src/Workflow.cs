using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class WorkflowImpl1 : Workflow
    {
        class ScopeVar1 : ScopeVariables
        {
            public int i;
        }

        public async Task Contagem(int i)
        {
            using (var scope = await ScopeAsync<ScopeVar1>("contagem"))
            {
                Bot bot = scope.Use<Bot>();

                Code("init", () =>
                {
                    bot.Send("Contagem2");
                    i = i + 1;
                });

                Code("list", () =>
                {
                    for (; i < 10; i++)
                    {
                        bot.Send("Hello " + i);
                    }
                });

                Code("list2", () =>
                {
                    Code("list2-botinit", () => { i = 0; });

                    while(i < 10)
                    {
                        Code("list2-botsend", () =>
                        {
                            bot.Send("Hello " + i);
                            i++;
                        });
                    }
                });

                Code("user", () =>
                {
                    string digi = bot.Recv();

                    while (digi != "1")
                    {
                        bot.Send("Type 1");
                        digi = bot.Recv();
                    }
                });
            }
        }
    }

    abstract class Workflow
    {
        protected Task<T> ScopeAsync<T>(string name)
            where T: IDisposable, new()
        {
            return Task.FromResult(new T());
        }

        protected void Code(string name, Action action)
        {
            Console.WriteLine("Step: " + name);
            action();
        }
    }

    abstract class ScopeVariables : IDisposable
    {
        Dictionary<Type,object> _instances = new Dictionary<Type, object>;

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

        public void Dispose()
        {
            foreach(var instance in _instances.Values)
            {
                var disposable = instance as IDisposable;
                if(instance != null)
                {
                    disposable.Dispose();
                }
            }
            _instances = null;
        }
    }
}

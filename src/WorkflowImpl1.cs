using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class ScopeVar1 : ScopeVariables
    {
        public int i;
    }

    class WorkflowImpl1 : Workflow
    {
        public async Task Contagem(int i)
        {
            using (var scope = await ScopeAsync<ScopeVar1>("contagem"))
            {
                Bot bot = scope.Use<Bot>();

                Code("init", () =>
                {
                    scope.i = i;

                    bot.Send("Contagem2");
                    scope.i = scope.i + 1;
                });

                Code("list", () =>
                {
                    for (; scope.i < 10; scope.i++)
                    {
                        bot.Send("Hello " + scope.i);
                    }
                });

                Code("list2", () =>
                {
                    Code("list2-botinit", () => { scope.i = i; });

                    while (scope.i < 10)
                    {
                        Code("list2-botsend", () =>
                        {
                            InjectFailureException.ThrowRate(50);
                            bot.Send("Hello " + scope.i);
                            scope.i++;
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
}

using System;
using System.Threading.Tasks;

namespace TaskFlow
{
    class Program
    {
        delegate void CodeCall(string name, Action<object> p);

        static void Main(string[] args)
        {
            var wf1 = new WorkflowImpl1();

            Workflow.RunAsync(async () =>
            {
                await wf1.Contagem(0);
            }).Wait();
            
        }
    }
}

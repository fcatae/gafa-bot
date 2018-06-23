using System;
using System.Threading.Tasks;

namespace TaskFlow
{
    class Program
    {
        delegate void CodeCall(string name, Action<object> p);

        static void Main(string[] args)
        {
            Runtime.RunAsync("TaskFlow.WorkflowImpl1", "Contagem", 0).Wait();                        
        }
    }
}

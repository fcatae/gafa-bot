using System;
using System.Threading.Tasks;

namespace TaskFlow
{
    class Program
    {
        delegate void CodeCall(string name, Action<object> p);

        static void Main(string[] args)
        {
            var ec = Runtime.Start("TaskFlow.WorkflowImpl1", "Contagem", 0);

            ec.GetTask().ContinueWith(t =>
            {
                Console.WriteLine("DONE");
            });

            var state = ec.GetState();

            ec.Continue();
        }
    }
}

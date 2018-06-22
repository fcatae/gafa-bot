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

            wf1.Contagem(0).Wait();
        }
    }
}

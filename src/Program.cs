using System;
using System.Threading.Tasks;

namespace TaskFlow
{
    class Program
    {
        delegate void CodeCall(string name, Action<object> p);

        static void Main(string[] args)
        {
            BotHub.StartConversationHub();

            var ec = Runtime.Start("TaskFlow.WorkflowImpl1", "Contagem", 0);

            ec.GetTask().ContinueWith(t =>
            {
                Console.WriteLine("DONE");
            });
                        
            var state = ec.GetState();
            bool hasFinished = ec.HasFinished();
            bool isRunning = ec.IsRunning();

            while (state != null)
            {
                Console.WriteLine("WorkflowInterruption: " + state);
                ec.Continue();
                Task.Delay(10000).Wait();

                state = ec.GetState();
            }

            
        }
    }
}

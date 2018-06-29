using System;

namespace Workflow
{
    class Program
    {
        static void Main(string[] args)
        {
            var azStorage = new AzureStorage();
            var azQueue = azStorage.CreateQueue("workflow");

            var workflow = new Workflow(azQueue);

            var client = workflow.GetClient();
            var server = workflow.GetServer();

            for(int i=0; i<10; i++)
            {
                client.Start("Workflow.HelloWorld", "Run", "Hello World " + i);
            }

            server.DoEventLoop();
        }
    }
}

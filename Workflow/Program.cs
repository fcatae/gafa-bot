using System;

namespace Workflow
{
    class Program
    {
        static void Main(string[] args)
        {
            var azQueue = new AzureStorageQueue();
            var workflow = new Workflow(azQueue);

            var client = workflow.GetClient();
            var server = workflow.GetServer();

            client.Start("Workflow.HelloWorld", "Run", "Hello World");

            server.DoEventLoop();
        }
    }
}

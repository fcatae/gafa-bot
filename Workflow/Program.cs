using System;

namespace Workflow
{
    class Program
    {
        static void Main(string[] args)
        {
            var workflow = new Workflow();

            var client = workflow.GetClient();
            var server = workflow.GetServer();

            client.Start("Workflow.HelloWorld", "Run", "Hello World");

            server.DoEventLoop();
        }
    }
}

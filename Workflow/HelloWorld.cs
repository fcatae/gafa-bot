using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    [WorkflowClass]
    class HelloWorld
    {
        public void Run(string param)
        {
            Console.WriteLine(param);
        }
    }
}

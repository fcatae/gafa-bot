using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    [WorkflowClass(timeout: 50)]
    class HelloWorld
    {
        static Random rnd = new Random();

        public void Run(string param)
        {   
            int fail = rnd.Next(0, 10);
            if (fail > 5)
                throw new InvalidOperationException();

            Console.WriteLine(param);
        }
    }
}

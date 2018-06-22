using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow
{
    class Bot
    {
        public void Send(string message)
        {
            Console.WriteLine("Bot: " + message);
        }

        public string Recv()
        {
            Console.Write("User: ");
            return Console.ReadLine();
        }
    }
}

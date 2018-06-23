using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow
{
    class Bot
    {
        Queue<string> _messages = new Queue<string>();

        public void Send(string message)
        {
            Console.WriteLine("Bot: " + message);
        }

        public string Recv()
        {
            //Console.Write("User: ");
            //return Console.ReadLine();
            return _messages.Dequeue();
        }

        public void Enqueue(string message)
        {
            _messages.Enqueue(message);
        }
    }
}

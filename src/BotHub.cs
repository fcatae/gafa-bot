using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class BotHub
    {
        static Dictionary<string, Bot> dict = new Dictionary<string, Bot>();

        public static void StartConversationHub()
        {
            Task.Run(() =>
            {
                while(true)
                {
                    //var key = Console.ReadKey();
                    //Console.Write("User: ");
                    string line = Console.ReadLine();

                    ConversationHub("1", line);
                }
            });
        }

        public static void ConversationHub(string id, string message)
        {
            if (!dict.ContainsKey(id))
            {
                dict.Add(id, new Bot());
            }

            Bot bot = dict[id];

            bot.Enqueue(message);
        }
    }
}

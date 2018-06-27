using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class UserBotHandler
    {
        readonly BotProxy bot;

        public UserBotHandler(BotProxy proxy)
        {
            bot = proxy;
        }

        public async Task RunAsync(string id)
        {
            for(int i=0; ;i++)
            {
                Console.WriteLine($"({id}) ... {i}");

                bot.Say($"({id}) Running... {i}");

                await Task.Delay(1000);
            }
        }
    }
}

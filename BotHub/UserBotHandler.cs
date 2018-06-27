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

        Nullable<int> DigiteUmNumero(string input)
        {
            if (Int32.TryParse(input, out int result))
            {
                return result;
            }

            bot.Say("Digite um número");
            return null;
        }

        public async Task RunAsync(string id)
        {
            for(int i=0; ;i++)
            {
                Console.WriteLine($"({id}) ... {i}");

                bot.Say($"Running... {i}");

                int? digi = await bot.Read(DigiteUmNumero);

                await Task.Delay(1000);
            }
        }
    }
}

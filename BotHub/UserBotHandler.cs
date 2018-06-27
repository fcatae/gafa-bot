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
        
        Nullable<bool> SimOuNao(string input)
        {
            if (input.ToLower().StartsWith("s"))
                return true;
        
            if (input.ToLower().StartsWith("n"))
                return false;

            bot.Say("Sim? Não?");
            return null;
        }

        public async Task Run2Async(string id)
        {
            Console.WriteLine($"({id}) Started");

            bot.Say($"Qual é seu nome?");
            string nome = await bot.Read();

            bot.Say($"Olá {nome}, eu sou o bot.");
            bot.Say($"Você sabe programar?");

            bool? resp = await bot.Read(SimOuNao);

            if( resp == true )
            {
                bot.Say("Você é muito inteligente.");
            }
            else
            {
                bot.Say("Quer aprender?");
                bool? resp2 = await bot.Read(SimOuNao);

                if (resp2 == true)
                {
                    bot.Say("Você é muito esforçado!");
                }
                else
                {
                    bot.Say("Você gosta de futebol?");
                    bool? resp3 = await bot.Read(SimOuNao);
                    bot.Say("Que interessante...");
                }
            }

            await Task.Delay(5000);

            bot.Say("Eu sei contar até 10..");

            for (int i=1; i<10;i++)
            {
                bot.Say($"{i}");

                await Task.Delay(1000);
            }

            bot.Say("E 10!!!");
        }
    }
}

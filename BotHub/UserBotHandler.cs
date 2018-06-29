using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BotHub
{
    class UserBotHandler : BotConversation
    {
        void Dialog(string name, Func<Task> action)
        {
        }

        Task UserIdle(object trigger)
        {
            return null;
        }

        Task<T> UserDelay<T>(Task<T> task)
        {
            return task;
        }

        Task<string> UserResponse()
        {
            return null;
        }

        Task<string> UserResponse<T>(Func<string, T> filter)
        {
            return null;
        }

        public async Task Dialog1()
        {
            string id = "1";


            Console.WriteLine($"({id}) Started");

            string nome = null;
            bool? resp = null;

            Dialog("intro", async () =>
            {
                bot.Say($"Qual é seu nome?");

                nome = await bot.Read();
            });

            Dialog("intro", async () =>
            {
                bot.Say($"Olá {nome}, eu sou o bot.");
                bot.Say($"Você sabe programar?");

                resp = await UserDelay(bot.Read(SimOuNao));
            });

            bot.Say($"Qual é seu nome?");

            await UserIdle(bot);

            nome = await bot.Read();

            bot.Say($"Olá {nome}, eu sou o bot.");
            bot.Say($"Você sabe programar?");

            var respsn = await UserResponse(SimOuNao);

        }

        public async Task Exemplo()
        {
            string id = "1";

            Console.WriteLine($"({id}) Started");

            await Checkpoint("intro");

            bot.Say($"Qual é seu nome?");
            
            string nome = await bot.Read();

            await Checkpoint("programar");

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

                await Checkpoint("aprender");

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

            await Checkpoint("contagem");

            bot.Say("Eu sei contar até 10..");

            for (int i=1; i<10;i++)
            {
                bot.Say($"{i}");

                await Task.Delay(1000);
            }

            bot.Say("E 10!!!");
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BotHub
{
    public class UserBotHandlerWaiter
    {
        readonly BotProxy bot;
        IAsyncStateMachine _state;

        public UserBotHandlerWaiter(BotProxy proxy)
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

        DurableFunction _f;

        public async Task RunAsync(string id)
        {
            _f = DurableFunction.Create(typeof(UserBotHandlerWaiter), "BotConversation", this);

            await _f.StartAsync();
            //await _f.StartAsync();
        }

        public Task Run<T>(string methodName)
        {
            var objType = typeof(T);
            var method = objType.GetMethod(methodName);
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
            var stateType = attrib.StateMachineType;
            
            // create type
            var stateObj = Activator.CreateInstance(stateType);
            var stateMachine = (IAsyncStateMachine)stateObj;

            // create the task builder
            var b = AsyncTaskMethodBuilder.Create();
            var taskField = stateType.GetField("<>t__builder");
            taskField.SetValue(stateMachine, b);

            // this field
            var thisField = stateType.GetField("<>4__this");
            thisField.SetValue(stateMachine, this);

            // set state
            var stateField = stateType.GetField("<>1__state");
            stateField.SetValue(stateMachine, -1);

            b.Start(ref stateMachine);

            _state = stateMachine;

            return b.Task;
        }

        UserAwaitable Checkpoint()
        {
            var ua = new UserAwaitable(_f);

            return ua;
        }

        async Task User(string name)
        {
            _f.Suspend();

            await bot.Read();

            _f.Resume();
        }

        private async Task BotConversationPrivc(string id)
        {
            await Task.Delay(1);
        }

        static public async Task BotConversationStatc(string id)
        {
            await Task.Delay(1);
        }

        public async Task BotConversation(string id)
        {
            Console.WriteLine($"({id}) Started");

            await Checkpoint();
            await Checkpoint();
            await Checkpoint();
            await User("check1");
            await User("check2");
            await User("check3");

            Console.WriteLine($"...");

            await User("a");

            Console.WriteLine($"({id}) Ended");
        }

        public async Task BotConversation2(string id)
        {
            Console.WriteLine($"({id}) Started");

            await User("intro");

            bot.Say($"Qual é seu nome?");
            
            await User("intro");

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

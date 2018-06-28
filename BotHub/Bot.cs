using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class Bot
    {
        BotProxy _botProxy;

        public Bot(BotProxy botProxy)
        {
            _botProxy = botProxy;
        }

        public void Say(string message)
        {
            _botProxy.Typing();
            Delay(1000);
            _botProxy.Say(message);
        }

        public Task<string> Read()
        {
            _botProxy.ClearInput();
            return _botProxy.Read();
        }

        public Task<T> Read<T>(Func<string, T> filter)
        {
            _botProxy.ClearInput();
            return _botProxy.Read<T>(filter);
        }

        void Delay(int milliseconds)
        {
            Task.Delay(milliseconds).Wait();
        }
    }
}

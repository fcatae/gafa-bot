using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    abstract class BotConversation
    {
        DurableFunction _durableFunction;

        public Bot bot { get; private set; }

        protected BotConversation()
        {
        }

        public void CreateBot(BotProxy proxy)
        {
            bot = new Bot(proxy);
        }

        public async Task StartAsync(string methodName = "Dialog")
        {
            _durableFunction = DurableFunction.Create(this.GetType(), methodName, this);

            await _durableFunction.StartAsync();
        }

        public CheckpointAwaiter Checkpoint(string name)
        {
            return new CheckpointAwaiter(name, _durableFunction);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class BotPostBack
    {
        readonly BotHubRoute _route;

        public BotPostBack(BotHubRoute route)
        {
            _route = route;
        }

        public void Send(BotMessage message)
        {
            _route.Send(message.Body);
        }
    }
}

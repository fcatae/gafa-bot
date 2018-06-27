using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class BotProxy
    {
        readonly BotQueue _receive;
        readonly BotPostBack _send;

        public BotProxy(BotQueue receive, BotPostBack send)
        {
            _receive = receive;
            _send = send;
        }

        public void Say(string message)
        {
            _send.Send(new BotMessage { Body = message });
        }
    }
}

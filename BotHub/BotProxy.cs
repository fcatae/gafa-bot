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

        public void Zero()
        {
            _receive.Clear();
        }

        public async Task<string> Read()
        {
            Zero();
            var message = await _receive.DequeueAsync();
            return message.Body;
        }

        public async Task<T> Read<T>(Func<string,T> filter)
        {
            Zero();
            while (true)
            {
                var message = await _receive.DequeueAsync();
                string text = message.Body;
                T result = filter(text);

                if (result != null)
                    return result;
            }
        }

    }
}

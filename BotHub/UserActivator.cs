﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class UserActivator
    {
        readonly string _id;
        readonly BotProxy _proxy;

        public UserActivator(string id, BotQueue receive, BotPostBack send)
        {
            _id = id;
            _proxy = new BotProxy(receive, send);
        }

        public void Start()
        {
            var handler = new UserBotHandlerWaiter(_proxy);

            Task.Run(() => handler.RunAsync(_id));            
        }
    }
}

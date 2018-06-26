using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow
{
    class BotConversation
    {
        public void Hello(Bot bot)
        {
            bot.Say("Hello");
            bot.Say("What do you want to do?");

            string intention = bot.Listen();
            
            switch (intention)
            {
                case "create":
                    bot.Say("Let's create something");

                    break;
            }
        }
    }
}

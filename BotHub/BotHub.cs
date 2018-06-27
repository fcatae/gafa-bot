using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class BotHub
    {
        Dictionary<string, BotHubRoute> _routes = new Dictionary<string, BotHubRoute>();

        public void Receive(Activity activity)
        {
            string channelId = activity.ChannelId;
            string conversationId = activity.Conversation.Id;
            string id = channelId + ":" + conversationId;

            var route = GetRoute(id);

            if( route == null )
            {
                route = CreateRoute(id, activity);
            }

            route.Enqueue(activity);
        }

        BotHubRoute GetRoute(string id)
        {
            _routes.TryGetValue(id, out BotHubRoute route);
            return route;
        }

        BotHubRoute CreateRoute(string id, Activity activity)
        {
            var route = new BotHubRoute(activity);
            var activator = new UserActivator(id, route.GetBotQueue(), route.GetBotPostBack());
            activator.Start();

            _routes[id] = route;
            return route;
        }

    }
}

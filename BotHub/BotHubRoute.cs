using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotHub
{
    public class BotHubRoute
    {
        private readonly string _channelId;
        private readonly string _conversationId;
        private readonly string _service;
        private readonly Uri _serviceUri;

        private readonly string _locale;
        private readonly ChannelAccount _botAccount;

        public BotHubRoute(Activity activity)
        {
            _channelId = activity.ChannelId;
            _conversationId = activity.Conversation.Id;
            _service = activity.ServiceUrl;
            _serviceUri = new Uri(activity.ServiceUrl);

            _locale = activity.Locale;
            _botAccount = activity.Recipient;
        }

        public void Send(string text)
        {
            var client = new ConnectorClient(_serviceUri);

            var reply = Activity.CreateMessageActivity();

            reply.Timestamp = DateTimeOffset.UtcNow;

            reply.ChannelId = _channelId;
            reply.Conversation = new ConversationAccount(id: _conversationId);
            reply.Locale = _locale;
            reply.From = _botAccount;
            reply.Text = text;

            client.Conversations.SendToConversation(_conversationId, (Activity)reply);
        }

        public void Enqueue(Activity activity)
        {
            var text = activity.Text;

            this.Send("OK " + text);
        }
    }
}

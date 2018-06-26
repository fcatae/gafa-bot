using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;

namespace BotHub.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        [HttpPost]
        public void Post([FromBody]Activity activity)
        {
            var message = activity.AsMessageActivity();

            if( message != null )
            {
                string text = activity.Text;

                var client = new ConnectorClient(new Uri(activity.ServiceUrl));

                var reply = activity.CreateReply("hello! " + text);

                client.Conversations.ReplyToActivity(reply);
            }

        }
    }
}

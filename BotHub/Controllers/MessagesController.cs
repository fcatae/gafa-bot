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
        public void Post([FromBody]Activity activity, [FromServices]BotHub bothub)
        {
            var message = activity.AsMessageActivity();

            if( message != null )
            {
                bothub.Receive(activity);
            }

        }
    }
}

using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using CoffeeBot.Dialogs;
using CoffeeBot.Services;
using CoffeeBot.Models;

namespace CoffeeBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            var attributes = new LuisModelAttribute(
                ConfigurationManager.AppSettings["LuisId"],
                ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
            var service = new LuisService(attributes);

            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    if (activity.Attachments?.Any() == true)
                    {
                        var reply = activity.CreateReply();
                        var contentUrl = activity.Attachments[0].ContentUrl;

                        AnalyzeResult analyze = await new VisaoComputacionalService().AnaliseDetalhadaAsync(contentUrl, activity.ServiceUrl);

                        if(analyze.tags.Select(t => t.name).Contains("coffee") || analyze.tags.Select(t => t.name).Contains("cup") ||
                           analyze.description.captions.FirstOrDefault().text.Contains("coffee") ||
                           analyze.description.captions.FirstOrDefault().text.Contains("cup"))
                        {
                            reply.Text = "Pela foto entendo que você queira um café, estou certo?";
                        } else
                        {
                            var description = analyze.description.captions.FirstOrDefault()?.text;
                            var confidence = analyze.description.captions.FirstOrDefault()?.confidence *100;

                            reply.Text = $"Não achei nenhum café na sua foto, mas tenho {(int)confidence}% de certeza que isso é *{description}*";
                        }
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    } else
                    {
                        await Conversation.SendAsync(activity, () => new LuisDialog(service, activity.ServiceUrl));
                    }
                    break;
                case ActivityTypes.ConversationUpdate:
                    if (activity.MembersAdded.Any(o => o.Id == activity.Recipient.Id))
                    {
                        var reply = activity.CreateReply();
                        reply.Text = "Olá, eu sou o **Coffee Bot**. Eu faço um café muito bom, qualquer dúvida é só me pedir **ajuda**";

                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    break;
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
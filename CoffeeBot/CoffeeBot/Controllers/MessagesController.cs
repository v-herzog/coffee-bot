﻿using System;
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
using static CoffeeBot.Servicos;
using System.IO;

namespace CoffeeBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
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
                        //reply.Text = await new VisaoComputacional().AnaliseDetalhadaAsync(new Uri(activity.Attachments[0].ContentUrl)) ? "Achei um café nessa foto" : "Não vejo nenhum café";
                        reply.Text = "eu deveria analisar sua imagem";

                        await connector.Conversations.ReplyToActivityAsync(reply);
                    } else
                    {
                        await Conversation.SendAsync(activity, () => new LuisDialog(service));
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
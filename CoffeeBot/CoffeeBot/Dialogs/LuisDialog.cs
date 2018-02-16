using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using static CoffeeBot.Servicos;

namespace CoffeeBot.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        public LuisDialog(ILuisService service) : base(service) { }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Ops, não entendi isso... ({result.Query})");
            context.Done<string>(null);
        }

        /// <summary>
        /// Quando não houve intenção reconhecida.
        /// </summary>
        [LuisIntent("")]
        public async Task IntencaoNaoReconhecida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**( ͡° ͜ʖ ͡°)**");
        }

        /// <summary>
        /// Caso a intenção não seja reconhecida.
        /// </summary>
        [LuisIntent("consciencia")]
        public async Task ConscienciaAsync(IDialogContext context, LuisResult result)
        {
            Activity resposta = ((Activity)context.Activity).CreateReply();

            HeroCard card = new HeroCard
            {
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "Saiba mais", value:"google.com")
                }
            };

            resposta.Attachments.Add(card.ToAttachment());

            await context.PostAsync("Eu sou um bot que faz café");
            await context.PostAsync("Fui criado pelo **Victor Damke** para a **Maratona Bots** da Microsoft.");
            await context.PostAsync(resposta);
        }

        /// <summary>
        /// Quando a intenção for por ajuda.
        /// </summary>
        [LuisIntent("ajudar")]
        public async Task AjudarAsync(IDialogContext context, LuisResult result)
        {
            var response = "Minha principal função é **fazer café**, é só pedir que eu faço um para você agora." +
                           " Você pode também só me mandar uma **foto da sua xícara vazia** que eu entendo o recado ;)";
            await context.PostAsync(response);
        }

        /// <summary>
        /// Quando a intenção for uma saudação.
        /// </summary>
        [LuisIntent("saudar")]
        public async Task Saudar(IDialogContext context, LuisResult result)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).TimeOfDay;
            string saudacao;

            if (now < TimeSpan.FromHours(12)) saudacao = "Bom dia!";
            else if (now < TimeSpan.FromHours(18)) saudacao = "Boa tarde!";
            else saudacao = "Boa noite!";

            Activity resposta = ((Activity)context.Activity).CreateReply();

            AdaptiveCard card = new AdaptiveCard();

            card.Body.Add(new TextBlock()
            {
                Text = $"{ saudacao } Você aceita um café?",
                Size = TextSize.Medium
            });

            card.Body.Add(new Image()
            {
                Url = "https://drnkcoffee.com/wp-content/uploads/2016/06/cogffee.png",
                Size = ImageSize.Auto
            });

            card.Actions.Add(new HttpAction()
            {
                Url = "http://192.168.0.101:8000/cafeteira/liga",
                Title = "Aceitar"
            });

            card.Actions.Add(new SubmitAction()
            {
                Title = "Recusar"
            });

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            resposta.Attachments.Add(attachment);
            await context.PostAsync(resposta);
        }

        /// <summary>
        /// Quando a intenção for descrever uma imagem.
        /// </summary>
        [LuisIntent("descrever-imagem")]
        public async Task DescreverImagen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, me envia a imagem a ser analisada.");
            context.Wait((c, a) => ProcessarImagemAsync(c, a));
        }

        /// <summary>
        /// Quando a intenção for pedir um café.
        /// </summary>
        [LuisIntent("pedir-cafe")]
        public async Task PedirCafe(IDialogContext context, LuisResult result)
        {
            string[] frases = { "Pelo que eu entendi você quer um café, certo?",
                                "Vou fazer um café pra você ok?",
                                "Beleza! Vou fazer um café para você, ok?",
                                "Então você gostaria de um café?",
                                "Você quer um café?" };

            int index = new Random().Next(0, frases.Length);
            Activity resposta = ((Activity)context.Activity).CreateReply();

            AdaptiveCard card = new AdaptiveCard();

            card.Body.Add(new TextBlock()
            {
                Text = frases[index],
                Size = TextSize.Medium
            });

            card.Body.Add(new Image()
            {
                Url = "https://drnkcoffee.com/wp-content/uploads/2016/06/cogffee.png",
                Size = ImageSize.Auto
            });

            card.Actions.Add(new HttpAction()
            {
                Url = "http://192.168.0.101:8000/cafeteira/liga",
                Title = "Aceitar"
            });

            card.Actions.Add(new SubmitAction()
            {
                Title = "Recusar"
            });

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            resposta.Attachments.Add(attachment);
            await context.PostAsync(resposta);
        }

        /// <summary>
        /// Quando a intenção for possibilidades.
        /// </summary>
        [LuisIntent("possibilidades")]
        public async Task Possibilidades(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Atualmente eu só faço café, no futuro talvez também faça chá...");
        }

        private async Task ProcessarImagemAsync(IDialogContext contexto, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument;

            var uri = activity.Attachments?.Any() == true ?
                new Uri(activity.Attachments[0].ContentUrl) :
                new Uri(activity.Text);

            try
            {
                string reply = await new VisaoComputacional().AnaliseDetalhadaAsync(uri) ?
                    "Achei um café nessa foto" : "Não vejo nenhum café";
                await contexto.PostAsync(reply);
            }
            catch (Exception)
            {
                await contexto.PostAsync("Errou! Oloco bixo, deu uma exception aqui.");
            }

            contexto.Wait(MessageReceived);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoffeeBot.Models;
using CoffeeBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace CoffeeBot.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private readonly string _github = ConfigurationManager.AppSettings["Github"];
        private readonly string _image = ConfigurationManager.AppSettings["Image"];
        private string ServiceUrl;

        public LuisDialog(ILuisService service, string serviceUrl) : base(service) {
            this.ServiceUrl = serviceUrl;
        }

        [LuisIntent("None")]
        public async Task NoneAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Ops, não entendi isso... ({result.Query})");
            context.Done<string>(null);
        }

        [LuisIntent("")]
        public async Task IntencaoNaoReconhecida(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("**( ͡° ͜ʖ ͡°)**");
        }

        [LuisIntent("consciencia")]
        public async Task ConscienciaAsync(IDialogContext context, LuisResult result)
        {
            Activity resposta = ((Activity)context.Activity).CreateReply();

            var cardButtons = new List<CardAction>
            {
                new CardAction()
                {
                    Value = _github,
                    Type = ActionTypes.OpenUrl,
                    Title = "Saiba mais..."
                }
            };

            HeroCard card = new HeroCard
            {
                Title = "Eu sou um bot que faz café",
                Subtitle = "Fui criado pelo Victor Damke para a Maratona Bots da Microsoft.",
                Buttons = cardButtons
            };

            resposta.Attachments.Add(card.ToAttachment());

            await context.PostAsync(resposta);
        }

        [LuisIntent("ajuda")]
        public async Task AjudarAsync(IDialogContext context, LuisResult result)
        {
            var response = "Minha principal função é **fazer café**, é só pedir que eu faço um para você agora." +
                           " Você pode também só me mandar uma **foto da sua xícara** que eu entendo o recado.";
            await context.PostAsync(response);
        }

        [LuisIntent("cumprimento")]
        public async Task Saudar(IDialogContext context, LuisResult result)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")).TimeOfDay;
            string saudacao;

            if (now < TimeSpan.FromHours(12)) saudacao = "Bom dia!";
            else if (now < TimeSpan.FromHours(18)) saudacao = "Boa tarde!";
            else saudacao = "Boa noite!";

            Activity resposta = ((Activity)context.Activity).CreateReply();
            resposta.Attachments = new List<Attachment>();

            HeroCard card = await CriarHeroCard(saudacao, true, false);

            resposta.Attachments.Add(card.ToAttachment());
            await context.PostAsync(resposta);
        }

        [LuisIntent("descreve-imagem")]
        public async Task DescreverImagen(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Me envie a url da imagem agora que eu descrevo o que vejo.");
            context.Wait((c, a) => ProcessarImagemAsync(c, a));
        }

        [LuisIntent("quero-cafe")]
        public async Task PedirCafe(IDialogContext context, LuisResult result)
        {
            string[] frases = { "Pelo que eu entendi você quer um café?",
                                "Vou fazer um café pra você ok?",
                                "Beleza! Vou fazer um café para você, ok?",
                                "Então você gostaria de um café?",
                                "Você quer um café?" };

            int index = new Random().Next(0, frases.Length);

            Activity resposta = ((Activity)context.Activity).CreateReply();
            resposta.Attachments = new List<Attachment>();

            HeroCard card = await CriarHeroCard(frases[index], false, true);

            resposta.Attachments.Add(card.ToAttachment());
            await context.PostAsync(resposta);
        }

        [LuisIntent("possibilidades")]
        public async Task Possibilidades(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Atualmente eu só faço café, no futuro talvez também faça chá...");
        }

        [LuisIntent("desliga-cafeteira")]
        public async Task DesligarCafeteira(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(await new CafeteiraService().EnviarAsync("desligar"));
        }

        [LuisIntent("positivo")]
        public async Task Positivo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(await new CafeteiraService().EnviarAsync("ligar"));
        }

        [LuisIntent("negativo")]
        public async Task Negativo(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Ok, me avise quando quiser um café.");
        }

        private async Task<HeroCard> CriarHeroCard(string titulo, bool comSubtitulo, bool comImagem)
        {
            var cardButtons = new List<CardAction>
            {
                new CardAction()
                {
                    Value = "Sim, por favor",
                    Type = ActionTypes.ImBack,
                    Title = "Aceitar"
                },
                new CardAction()
                {
                    Value = "Não, obrigado",
                    Type = ActionTypes.ImBack,
                    Title = "Recusar"
                }
            };

            string subtitulo = "Você aceita um café?";

            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: _image));

            var card = new HeroCard()
            {
                Title = titulo,
                Subtitle = comSubtitulo ? subtitulo : null,
                Images = comImagem ? cardImages : null,
                Buttons = cardButtons,
            };

            return card;
        }

        private async Task ProcessarImagemAsync(IDialogContext contexto, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            try
            {
                AnalyzeResult analyze = await new VisaoComputacionalService().AnaliseDetalhadaAsync(message.Text, this.ServiceUrl);

                var description = analyze.description.captions.FirstOrDefault()?.text;
                var confidence = analyze.description.captions.FirstOrDefault()?.confidence;

                string confidenceInText;

                if (confidence >= .9)
                    confidenceInText = "Tenho certeza que é";

                else if (confidence < .9 && confidence > .6)
                    confidenceInText = "Acho que pode ser";

                else
                    confidenceInText = "Talvez seja";

                string reply = $"{confidenceInText} *{description}*";

                await contexto.PostAsync(reply);
            }
            catch (Exception)
            {
                await contexto.PostAsync("Houve algum erro ao analisar sua imagem.");
            }

            contexto.Wait(MessageReceived);
        }
    }
}
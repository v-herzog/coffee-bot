using CoffeeBot.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace CoffeeBot.Services
{
    public class CafeteiraService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string _cafeteriaUri = ConfigurationManager.AppSettings["CafeteiraUri"];

        public async Task<string> EnviarAsync(string comando)
        {
            var body = new FormUrlEncodedContent(new Dictionary<string, string> { });

            try
            {
                var response = await client.PostAsync(string.Concat(_cafeteriaUri, comando), body);

                Cafeteira cafeteira = await response.Content.ReadAsAsync<Cafeteira>(new[] { new JsonMediaTypeFormatter() });

                string mensagem;

                if (cafeteira.Status.Equals("ON") && comando.Equals("ligar"))
                {
                    mensagem = "**Sucesso!** Pedi para sua cafeteira fazer um café!";
                }
                else if (cafeteira.Status.Equals("OFF") && comando.Equals("desligar"))
                {
                    mensagem = "Desliguei sua cafeteira.";
                }
                else
                {
                    mensagem = $"Algo deu errado e não consegui { comando } sua cafeteira";
                }

                return mensagem;
            }
            catch (HttpRequestException e)
            {
                return "Não consegui achar sua cafeteira, confira se ela está conectada a internet.";
            }
        }
    }
}
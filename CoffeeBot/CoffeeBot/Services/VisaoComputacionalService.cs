using CoffeeBot.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoffeeBot.Services
{
    public class VisaoComputacionalService
    {
        private readonly string _computerVisionApiKey = ConfigurationManager.AppSettings["ComputerVisionApiKey"];
        private readonly string _computerVisionUri = ConfigurationManager.AppSettings["ComputerVisionUri"];

        public async Task<bool> AnaliseDetalhadaAsync(Uri query)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _computerVisionApiKey);

            HttpResponseMessage response = null;

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Tags";

            var byteData = Encoding.UTF8.GetBytes("{ 'url': '" + query + "' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync($"{_computerVisionUri}?{queryString}",
                    content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var analise = JsonConvert.DeserializeObject<AnalyzeResult>(responseString);

            return analise.tags.Select(t => t.name).Contains("coffee") ||
                   analise.tags.Select(t => t.name).Contains("cup");
        }
    }
}
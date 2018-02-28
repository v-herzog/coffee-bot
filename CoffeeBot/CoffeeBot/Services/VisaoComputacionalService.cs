using CoffeeBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
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

        public async Task<AnalyzeResult> AnalyzeUrl(byte[] attachmentData)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _computerVisionApiKey);

            HttpResponseMessage response = null;

            string queryString = "visualFeatures=Tags,Description";

            using (var content = new ByteArrayContent(attachmentData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync($"{_computerVisionUri}?{queryString}",
                    content).ConfigureAwait(false);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AnalyzeResult>(responseString);
        }

        public async Task<AnalyzeResult> AnaliseDetalhadaAsync(string contentUrl, string serviceUrl)
        {
            using (var connectorClient = new ConnectorClient(new Uri(serviceUrl)))
            {
                var token = await (connectorClient.Credentials as MicrosoftAppCredentials).GetTokenAsync();
                var uri = new Uri(contentUrl);
                using (var httpClient = new HttpClient())
                {
                    var content = connectorClient.HttpClient.GetStreamAsync(contentUrl).R‌​esult;
                    var memoryStream = new MemoryStream();
                    content.CopyTo(memoryStream);
                    return await AnalyzeUrl(memoryStream.ToArray());
                }
            }
        }
    }
}
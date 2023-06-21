using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Getter
{
    public class HttpGetter
    {
        private readonly HttpClient httpClient;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<HttpGetter> logger;

        public HttpGetter(IHttpClientFactory clientFactory, ILogger<HttpGetter> logger)
        {
            this.clientFactory = clientFactory;
            this.logger = logger;
            this.httpClient = this.clientFactory.CreateClient("PollyEnabledClient");
        }

        public async Task<string> GetAsync(string url)
        {
            logger.BeginScope($"GETTER - Requesting {url}");
            {
                logger.LogInformation($"Requesting {url}");
            }
            var response = await this.httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace AzureFunctionsDemo.NewsApi
{
    public class NewsApiClient
    {
        private readonly HttpClient _httpClient;

        public NewsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NewsApiResponse> GetTopHeadlinesAsync(string apiKey, string category, string country)
        {
            _httpClient.DefaultRequestHeaders.Add("user-agent", "News-API-csharp/0.1");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var requestUri = $"https://newsapi.org/v2/top-headlines?country={country}&category={category}&apiKey={apiKey}";
            var response = await _httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NewsApiResponse>(content);
        }
    }
}

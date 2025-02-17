using System.Text.Json.Serialization;

namespace AzureFunctionsDemo.NewsApi
{
    public class NewsApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }
        [JsonPropertyName("articles")]
        public Article[] Articles { get; set; }
    }
}

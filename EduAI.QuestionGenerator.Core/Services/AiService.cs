using EduAI.QuestionGenerator.Core.IServiceContract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EduAI.QuestionGenerator.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiService> _logger;
        private readonly string _apiKey;

        private const string ModelEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";


        public AiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API Key not configured");
        }

        public async Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.95,
                    maxOutputTokens = 8192
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending prompt to Gemini...");

            var response = await _httpClient.PostAsync(
                $"{ModelEndpoint}?key={_apiKey}",
                content,
                cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini error: {Status} - {Body}", response.StatusCode, responseText);
                throw new HttpRequestException($"Gemini Error: {response.StatusCode}\n{responseText}");
            }

            _logger.LogInformation("Gemini responded successfully.");

            using var doc = JsonDocument.Parse(responseText);

            var result = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return result?.Trim('`', ' ', '\n') ?? "";
        }
    }
}

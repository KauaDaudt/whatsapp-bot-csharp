using System.Text;
using System.Text.Json;
using WhatsAppBot.Models;

namespace WhatsAppBot.Services;

public class ClaudeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetReplyAsync(string systemPrompt, List<Conversation> history, string newMessage)
    {
        var apiKey = _configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey not configured.");
        var client = _httpClientFactory.CreateClient("Groq");

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        messages.AddRange(history.Select(m => new { role = m.Role, content = m.Content }));
        messages.Add(new { role = "user", content = newMessage });

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq API error {Status}: {Body}", response.StatusCode, responseBody);
            return "Sorry, I could not process your request right now.";
        }

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text ?? string.Empty;
    }
}

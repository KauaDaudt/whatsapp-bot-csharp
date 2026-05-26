using System.Text;
using System.Text.Json;

namespace WhatsAppBot.Services;

public class WhatsAppService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WhatsAppService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTextAsync(string evolutionApiUrl, string evolutionApiKey, string toPhone, string text)
    {
        var instanceName = _configuration["Evolution:InstanceName"]
            ?? throw new InvalidOperationException("Evolution:InstanceName not configured.");

        var client = _httpClientFactory.CreateClient("Evolution");

        var url = $"{evolutionApiUrl.TrimEnd('/')}/message/sendText/{instanceName}";

        var body = new
        {
            number = toPhone,
            textMessage = new { text = text }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("apikey", evolutionApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();
            _logger.LogError("Evolution API error {Status}: {Detail}", response.StatusCode, detail);
        }
    }
}

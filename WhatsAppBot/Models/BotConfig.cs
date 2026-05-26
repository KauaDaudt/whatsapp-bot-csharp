namespace WhatsAppBot.Models;

public class BotConfig
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string EvolutionApiUrl { get; set; } = string.Empty;
    public string EvolutionApiKey { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

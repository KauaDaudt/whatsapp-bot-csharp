using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services;

namespace WhatsAppBot.Controllers;

[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClaudeService _claude;
    private readonly WhatsAppService _whatsApp;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(AppDbContext db, ClaudeService claude, WhatsAppService whatsApp, ILogger<WebhookController> logger)
    {
        _db = db;
        _claude = claude;
        _whatsApp = whatsApp;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JsonElement payload)
    {
        try
        {
            Console.WriteLine("[WEBHOOK RAW] " + payload.GetRawText());

            // Only handle messages.upsert events
            var eventType = payload.TryGetProperty("event", out var ev) ? ev.GetString() : null;
            if (eventType != "messages.upsert")
                return Ok();

            var data = payload.GetProperty("data");

            // Ignore outgoing messages
            var fromMe = data.TryGetProperty("key", out var key) &&
                         key.TryGetProperty("fromMe", out var fromMeProp) &&
                         fromMeProp.GetBoolean();

            if (fromMe)
                return Ok();

            var remoteJid = key.GetProperty("remoteJid").GetString() ?? string.Empty;

            var senderJid = payload.TryGetProperty("sender", out var senderProp)
                ? senderProp.GetString() ?? remoteJid
                : remoteJid;

            var phone = senderJid.Replace("@s.whatsapp.net", "").Replace("@g.us", "");

            // Extract message text
            string? messageText = null;
            if (data.TryGetProperty("message", out var message))
            {
                if (message.TryGetProperty("conversation", out var conv))
                    messageText = conv.GetString();
                else if (message.TryGetProperty("extendedTextMessage", out var ext) &&
                         ext.TryGetProperty("text", out var extText))
                    messageText = extText.GetString();
            }

            if (string.IsNullOrWhiteSpace(messageText))
                return Ok();

            // Load config
            var config = await _db.BotConfigs.OrderBy(c => c.Id).FirstOrDefaultAsync();
            if (config == null)
                return Ok();

            // Load last 20 conversation messages
            var history = await _db.Conversations
                .Where(c => c.PhoneNumber == phone)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            // Get reply from Claude
            var reply = await _claude.GetReplyAsync(config.SystemPrompt, history, messageText);

            // Persist user message and assistant reply
            _db.Conversations.Add(new Conversation
            {
                PhoneNumber = phone,
                Role = "user",
                Content = messageText,
                CreatedAt = DateTime.UtcNow
            });

            _db.Conversations.Add(new Conversation
            {
                PhoneNumber = phone,
                Role = "assistant",
                Content = reply,
                CreatedAt = DateTime.UtcNow.AddMilliseconds(1)
            });

            await _db.SaveChangesAsync();

            // Send reply via Evolution API
            var sendTo = payload.TryGetProperty("sender", out var sendToProp)
                ? sendToProp.GetString() ?? remoteJid
                : remoteJid;

            await _whatsApp.SendTextAsync(config.EvolutionApiUrl, config.EvolutionApiKey, sendTo, reply);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in webhook");
        }

        // Always 200 OK
        return Ok();
    }
}

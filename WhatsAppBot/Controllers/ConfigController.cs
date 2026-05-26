using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;

namespace WhatsAppBot.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConfigController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var config = await _db.BotConfigs.OrderBy(c => c.Id).FirstOrDefaultAsync();
        if (config == null)
            return NotFound();
        return Ok(config);
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] BotConfig incoming)
    {
        var config = await _db.BotConfigs.OrderBy(c => c.Id).FirstOrDefaultAsync();

        if (config == null)
        {
            incoming.Id = 0;
            incoming.UpdatedAt = DateTime.UtcNow;
            _db.BotConfigs.Add(incoming);
        }
        else
        {
            config.PhoneNumber = incoming.PhoneNumber;
            config.SystemPrompt = incoming.SystemPrompt;
            config.EvolutionApiUrl = incoming.EvolutionApiUrl;
            config.EvolutionApiKey = incoming.EvolutionApiKey;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(config ?? incoming);
    }

    [HttpDelete("history/{phone}")]
    public async Task<IActionResult> ClearHistory(string phone)
    {
        var messages = await _db.Conversations
            .Where(c => c.PhoneNumber == phone)
            .ToListAsync();

        _db.Conversations.RemoveRange(messages);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = messages.Count });
    }
}

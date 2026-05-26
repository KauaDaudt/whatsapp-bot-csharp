using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;

namespace WhatsAppBot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BotConfig> BotConfigs => Set<BotConfig>();
    public DbSet<Conversation> Conversations => Set<Conversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => c.PhoneNumber);

        modelBuilder.Entity<BotConfig>().HasData(new BotConfig
        {
            Id = 1,
            PhoneNumber = "",
            SystemPrompt = "You are a helpful assistant.",
            EvolutionApiUrl = "http://localhost:8080",
            EvolutionApiKey = "",
            UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

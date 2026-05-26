using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=whatsbot.db"));

builder.Services.AddHttpClient("Groq");
builder.Services.AddHttpClient("Evolution");

builder.Services.AddScoped<ClaudeService>();
builder.Services.AddScoped<WhatsAppService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

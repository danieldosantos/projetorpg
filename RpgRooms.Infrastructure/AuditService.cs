using System.Text.Json;
using RpgRooms.Core.Entities;

namespace RpgRooms.Infrastructure;

public class AuditService
{
    private readonly ApplicationDbContext _db;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task LogAsync(Campaign campaign, ApplicationUser user, string action, object details)
    {
        var json = JsonSerializer.Serialize(details, _jsonOptions);
        var entry = new AuditEntry
        {
            CampaignId = campaign.Id,
            Campaign = campaign,
            UserId = user.Id,
            User = user,
            Action = action,
            Data = json,
            CreatedAt = DateTime.UtcNow
        };
        _db.AuditEntries.Add(entry);
        return Task.CompletedTask;
    }
}

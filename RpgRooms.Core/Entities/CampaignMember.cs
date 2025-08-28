using System;

namespace RpgRooms.Core.Entities;

public class CampaignMember
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

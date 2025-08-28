using System;

namespace RpgRooms.Core.Entities;

public class JoinRequest
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}

using System;
using System.Collections.Generic;

namespace RpgRooms.Core.Entities;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser? OwnerUser { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Active;
    public bool IsRecruiting { get; set; }
    public int MaxPlayers { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }

    public ICollection<CampaignMember> Members { get; set; } = new List<CampaignMember>();
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
}

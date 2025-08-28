using Microsoft.AspNetCore.Identity;

namespace RpgRooms.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsGameMaster { get; set; }
    public ICollection<Campaign> OwnedCampaigns { get; set; } = new List<Campaign>();
    public ICollection<CampaignMember> CampaignMemberships { get; set; } = new List<CampaignMember>();
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
}


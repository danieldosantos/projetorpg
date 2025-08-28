using RpgRooms.Core.Entities;

namespace RpgRooms.Web.Models;

public class CampaignSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int MaxPlayers { get; set; }
    public bool IsRecruiting { get; set; }
    public CampaignStatus Status { get; set; }
}

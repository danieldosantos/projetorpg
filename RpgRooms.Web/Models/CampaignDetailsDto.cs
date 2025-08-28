using RpgRooms.Core.Entities;

namespace RpgRooms.Web.Models;

public class CampaignDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecruiting { get; set; }
    public CampaignStatus Status { get; set; }
    public List<string> Members { get; set; } = new();
    public List<ChatMessageDto> Chat { get; set; } = new();
}

public class ChatMessageDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

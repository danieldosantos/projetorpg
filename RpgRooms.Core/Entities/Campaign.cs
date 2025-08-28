namespace RpgRooms.Core.Entities;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GameMasterId { get; set; } = string.Empty;
    public ApplicationUser? GameMaster { get; set; }
    public bool IsFinished { get; set; }
    public ICollection<ApplicationUser> Players { get; set; } = new List<ApplicationUser>();
}

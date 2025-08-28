namespace RpgRooms.Core.Entities;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GameMasterId { get; set; }
    public User? GameMaster { get; set; }
    public bool IsFinished { get; set; }
    public ICollection<User> Players { get; set; } = new List<User>();
}

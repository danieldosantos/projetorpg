using Microsoft.AspNetCore.Identity;

namespace RpgRooms.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsGameMaster { get; set; }
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}


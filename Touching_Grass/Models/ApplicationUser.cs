using Microsoft.AspNetCore.Identity;

namespace Touching_Grass.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string DefaultZipCode { get; set; } = string.Empty;

    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
}
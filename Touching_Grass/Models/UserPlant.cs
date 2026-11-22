namespace Touching_Grass.Models;

public sealed class UserPlant
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public int SourcePlantId { get; set; }

    public string CommonNameSnapshot { get; set; } = string.Empty;

    public string ScientificNameSnapshot { get; set; } = string.Empty;

    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSearchedAt { get; set; }
}
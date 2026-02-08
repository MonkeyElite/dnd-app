namespace DndApp.Identity.Options;

public sealed class DevSeedOptions
{
    public const string SectionName = "DevSeed";

    public bool Enabled { get; init; }

    public string AdminUsername { get; init; } = "admin";

    public string AdminPassword { get; init; } = "admin";

    public string AdminDisplayName { get; init; } = "Platform Admin";
}

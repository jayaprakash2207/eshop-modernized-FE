namespace PlatformApp.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string Provider { get; init; } = "InMemory";
    public string ConnectionString { get; init; } = "Host=postgres;Port=5432;Database=platformapp;Username=platformapp;Password=platformapp";
}

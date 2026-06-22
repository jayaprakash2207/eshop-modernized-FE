using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Catalog;

public sealed class CatalogType : Entity
{
    private CatalogType()
    {
    }

    public CatalogType(string name)
    {
        Name = ValidateName(name);
    }

    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        Name = ValidateName(name);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static CatalogType Restore(Guid id, string name)
    {
        var type = new CatalogType(name);
        type.RestoreIdentity(id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        type.ClearDomainEvents();
        return type;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Catalog type name is required.", nameof(name));
        }

        return name.Trim();
    }
}

using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Catalog;

public sealed class CatalogBrand : Entity
{
    private CatalogBrand()
    {
    }

    public CatalogBrand(string name)
    {
        Name = ValidateName(name);
    }

    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        Name = ValidateName(name);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static CatalogBrand Restore(Guid id, string name)
    {
        var brand = new CatalogBrand(name);
        brand.RestoreIdentity(id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        brand.ClearDomainEvents();
        return brand;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Catalog brand name is required.", nameof(name));
        }

        return name.Trim();
    }
}

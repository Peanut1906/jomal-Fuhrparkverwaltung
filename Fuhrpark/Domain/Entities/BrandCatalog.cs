using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class BrandCatalog
{
    private readonly Dictionary<string, Brand> _brands = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<Brand> Brands => _brands.Values;

    public void AddBrand(string brandName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));
        if (!_brands.ContainsKey(b))
            _brands[b] = new Brand(b);
    }

    public void AddModel(string brandName, string modelName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));
        var m = Guard.NotNullOrWhiteSpace(modelName, nameof(modelName));

        if (!_brands.TryGetValue(b, out var brand))
        {
            brand = new Brand(b);
            _brands[b] = brand;
        }

        brand.AddModel(m);
    }

    public bool TryGetBrand(string brandName, out Brand brand)
        => _brands.TryGetValue(brandName, out brand!);

    public bool IsKnownModel(string brandName, string modelName)
        => _brands.TryGetValue(brandName, out var brand) && brand.HasModel(modelName);
}
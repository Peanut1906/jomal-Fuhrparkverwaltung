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

    public bool RemoveBrand(string brandName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));
        return _brands.Remove(b);
    }

    public bool RemoveModel(string brandName, string modelName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));
        var m = Guard.NotNullOrWhiteSpace(modelName, nameof(modelName));

        if (!_brands.TryGetValue(b, out var brand))
            return false;

        return brand.RemoveModel(m);
    }

    public bool TryAddBrand(string brandName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));

        if (_brands.ContainsKey(b))
            return false;

        _brands[b] = new Brand(b);
        return true;
    }

    public bool TryAddModel(string brandName, string modelName)
    {
        var b = Guard.NotNullOrWhiteSpace(brandName, nameof(brandName));
        var m = Guard.NotNullOrWhiteSpace(modelName, nameof(modelName));

        if (!_brands.TryGetValue(b, out var brand))
        {
            brand = new Brand(b);
            _brands[b] = brand;
        }

        return brand.TryAddModel(m);
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
using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Domain.Exceptions;

namespace Fuhrpark.Application.Services;

public sealed class BrandCatalogService
{
    private readonly IBrandCatalogRepository _repo;
    private BrandCatalog _catalog;

    public BrandCatalogService(IBrandCatalogRepository repo)
    {
        _repo = repo;
        _catalog = _repo.Load();
    }

    public IReadOnlyList<string> GetBrands()
        => _catalog.Brands
            .Select(b => b.Name)
            .OrderBy(x => x)
            .ToList();

    public IReadOnlyList<string> GetModels(string brandName)
    {
        if (!_catalog.TryGetBrand(brandName, out var brand))
            return new List<string>();

        return brand.Models
            .OrderBy(x => x)
            .ToList();
    }

    public void AddBrand(string brandName)
    {
        _catalog.AddBrand(brandName);
        _repo.Save(_catalog);
    }

    public void AddModel(string brandName, string modelName)
    {
        _catalog.AddModel(brandName, modelName);
        _repo.Save(_catalog);
    }

    public void EnsureKnownBrandModel(string brandName, string modelName)
    {
        if (!_catalog.IsKnownModel(brandName, modelName))
            throw new DomainValidationException($"Unbekannte Kombination: Marke '{brandName}' / Modell '{modelName}'. Bitte zuerst als Stammdaten anlegen.");
    }
}
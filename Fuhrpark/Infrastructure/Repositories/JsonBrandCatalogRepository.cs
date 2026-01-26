using Fuhrpark.Application.Interfaces;
using Fuhrpark.Domain.Entities;
using Fuhrpark.Infrastructure.Dtos;
using Fuhrpark.Infrastructure.Persistence;

namespace Fuhrpark.Infrastructure.Repositories;

public sealed class JsonBrandCatalogRepository : IBrandCatalogRepository
{
    private readonly string _path;

    public JsonBrandCatalogRepository(string path)
    {
        _path = path;
    }

    public BrandCatalog Load()
    {
        var dtos = JsonFileStore.Load(_path, new List<BrandDto>());
        var catalog = new BrandCatalog();

        foreach (var dto in dtos)
        {
            catalog.AddBrand(dto.Name);
            foreach (var model in dto.Models)
                catalog.AddModel(dto.Name, model);
        }

        return catalog;
    }

    public void Save(BrandCatalog catalog)
    {
        var dtos = catalog.Brands
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto
            {
                Name = b.Name,
                Models = b.Models.OrderBy(m => m).ToList()
            })
            .ToList();

        JsonFileStore.Save(_path, dtos);
    }
}
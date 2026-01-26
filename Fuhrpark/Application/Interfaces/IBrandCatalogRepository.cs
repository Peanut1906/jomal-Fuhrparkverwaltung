using Fuhrpark.Domain.Entities;

namespace Fuhrpark.Application.Interfaces;

public interface IBrandCatalogRepository
{
    BrandCatalog Load();
    void Save(BrandCatalog catalog);
}
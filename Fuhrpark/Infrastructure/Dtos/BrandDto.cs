namespace Fuhrpark.Infrastructure.Dtos;

internal sealed class BrandDto
{
    public string Name { get; set; } = "";
    public List<string> Models { get; set; } = new();
}
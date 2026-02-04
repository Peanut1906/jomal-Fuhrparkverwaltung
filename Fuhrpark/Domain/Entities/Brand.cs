using Fuhrpark.Domain.Validation;

namespace Fuhrpark.Domain.Entities;

public sealed class Brand
{
    public string Name { get; }

    private readonly HashSet<string> _models = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<string> Models => _models;

    public Brand(string name, IEnumerable<string>? models = null)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));

        if (models != null)
        {
            foreach (var m in models)
                AddModel(m);
        }
    }

    public bool RemoveModel(string modelName)
    {
        var m = Guard.NotNullOrWhiteSpace(modelName, nameof(modelName));
        return _models.Remove(m);
    }

    public void AddModel(string modelName)
    {
        var m = Guard.NotNullOrWhiteSpace(modelName, nameof(modelName));
        _models.Add(m);
    }

    public bool HasModel(string modelName)
        => _models.Contains(modelName);
}
using System.Text.Json;

namespace Fuhrpark.Infrastructure.Persistence;

internal static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static T Load<T>(string path, T defaultValue)
    {
        if (!File.Exists(path))
            return defaultValue;

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<T>(json, Options);
        return data ?? defaultValue;
    }

    public static void Save<T>(string path, T data)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(path, json);
    }
}
using System.Text.Json;

namespace DndApp.Campaign.Serialization;

public static class JsonArraySerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(IReadOnlyCollection<T> values)
        => JsonSerializer.Serialize(values, JsonSerializerOptions);

    public static List<T> Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonSerializerOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

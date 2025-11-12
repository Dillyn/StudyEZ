using System.Text.Json;

namespace studyez_backend.Core.Auth
{
    public static class JsonElementExtensions
    {
        public static string? TryGetString(this JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
                return null;

            if (element.TryGetProperty(propertyName, out var p))
                return p.ValueKind switch
                {
                    JsonValueKind.String => p.GetString(),
                    JsonValueKind.Number => p.ToString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                };

            return null;
        }
    }
}

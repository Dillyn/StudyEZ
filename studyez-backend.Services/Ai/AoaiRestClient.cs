using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using studyez_backend.Core.Interfaces;

namespace studyez_backend.Services.Ai
{
    public sealed class AoaiRestClient(HttpClient http, IOptions<AzureOpenAIOptions> opt) : IAoaiRestClient
    {
        private readonly HttpClient _http = http;
        private readonly AzureOpenAIOptions _opt = opt.Value;

        public async Task<string> ChatAsync(string deployment, object body, CancellationToken ct = default)
        {
            if (_opt.UseGlobalEndpoint)
                throw new InvalidOperationException("Global endpoint disabled. Set UseGlobalEndpoint=false and pass deployment names.");

            var uri = $"openai/deployments/{deployment}/chat/completions?api-version={_opt.ApiVersion}";

            using var resp = await _http.PostAsJsonAsync(uri, body, JsonOpts, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"AOAI {resp.StatusCode} {resp.ReasonPhrase}. Body: {raw}");

            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
}


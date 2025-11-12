using studyez_backend.Core.Interfaces;

namespace studyez_backend.Services.Ai
{
    public sealed class AiClient : IAiClient
    {
        public Task<string> SimplifyAsync(string content, CancellationToken ct)
            => Task.FromResult($"[simplified] {content}");
    }
}

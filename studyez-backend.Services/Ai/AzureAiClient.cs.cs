using Microsoft.Extensions.Options;
using studyez_backend.Core.Interfaces;

namespace studyez_backend.Services.Ai
{
    public sealed class AzureAiClient : IAiClient
    {
        private readonly IAoaiRestClient _rest;
        private readonly AzureOpenAIOptions _opt;

        public AzureAiClient(IAoaiRestClient rest, IOptions<AzureOpenAIOptions> opt)
        {
            _rest = rest;
            _opt = opt.Value;
        }

        public async Task<string> SimplifyAsync(string content, CancellationToken ct = default)
        {
            const int MaxChunkChars = 8000;

            // Chunk the content into smaller parts if it exceeds the max chunk size
            var chunks = Chunk(content, MaxChunkChars);

            var sys = "You are a study simplifier. Output clear Markdown with headings/bullets; no external facts.";

            // Initialize a list to store the simplified parts of the content
            var parts = new List<string>(chunks.Count);

            // Loop through each chunk, sending it to the model for simplification.
            foreach (var ch in chunks)
            {

                // Construct the request body for the API call, including the system and user messages.
                var body = new
                {
                    messages = new object[]
                    {
                    new { role = "system", content = sys },
                    new { role = "user",   content = ch  }
                    },
                    temperature = 1,
                    max_completion_tokens = 1500
                };

                var id = _opt.SimplifyModelOrDeployment;
                var txt = await _rest.ChatAsync(id, body, ct);
                parts.Add(txt.Trim());
            }

            if (parts.Count == 1) return parts[0];

            var mergeBody = new
            {
                messages = new object[]
                {
                new { role = "system", content = "Merge sections into one cohesive Markdown. Remove duplicates; keep concise." },
                new { role = "user",   content = string.Join("\n\n---\n\n", parts) }
                },
                temperature = 1,
                max_completion_tokens = 2000
            };

            // Send the merge request to the API and return the merged content.
            return await _rest.ChatAsync(_opt.SimplifyModelOrDeployment, mergeBody, ct);
        }

        // Helper method to split long content into chunks
        private static List<string> Chunk(string text, int size)
        {
            var list = new List<string>(Math.Max(1, text.Length / size + 1));
            for (int i = 0; i < text.Length; i += size)
                list.Add(text.Substring(i, Math.Min(size, text.Length - i)));
            return list;
        }
    }
}

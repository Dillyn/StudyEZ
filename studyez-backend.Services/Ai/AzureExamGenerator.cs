using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using studyez_backend.Core.Interfaces;

namespace studyez_backend.Services.Ai
{

    public sealed class AzureExamGenerator : IExamGenerator
    {
        private readonly IAoaiRestClient _rest;
        private readonly AzureOpenAIOptions _opt;

        public AzureExamGenerator(IAoaiRestClient rest, IOptions<AzureOpenAIOptions> opt)
        {
            _rest = rest;
            _opt = opt.Value;
        }

        public async Task<GeneratedExamResult> GenerateAsync(
            string courseName,
            IEnumerable<(string ModuleTitle, string OriginalContent)> modules,
            int totalQuestions = 20,
            CancellationToken ct = default)
        {
            int mcq = (int)Math.Round(totalQuestions * 0.70, MidpointRounding.AwayFromZero);
            int tf = (int)Math.Round(totalQuestions * 0.20, MidpointRounding.AwayFromZero);
            int sa = totalQuestions - mcq - tf;

            var prompt = BuildPrompt(courseName, modules, totalQuestions, mcq, tf, sa);

            var body = new
            {
                messages = new object[]
                {
                new { role = "system", content = "Return STRICT JSON only (no extra text)." },
                new { role = "user",   content = prompt }
                },
                temperature = 1,
                max_completion_tokens = 4000,
                response_format = new { type = "json_object" }
            };

            var raw = await _rest.ChatAsync(_opt.ExamModelOrDeployment, body, ct);

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var title = root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
            var items = new List<GeneratedExamItem>();

            if (root.TryGetProperty("items", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in arr.EnumerateArray())
                {
                    var type = el.GetProperty("type").GetString()!;
                    var q = el.GetProperty("questionText").GetString()!;
                    var a = el.GetProperty("correctAnswer").GetString()!;
                    var ord = el.GetProperty("order").GetInt32();

                    string[]? options = null;
                    if (type == "multiple-choice" && el.TryGetProperty("options", out var optEl) && optEl.ValueKind == JsonValueKind.Array)
                    {
                        options = optEl.EnumerateArray().Select(x => x.GetString() ?? "").ToArray();
                        options = options.Length >= 4 ? options.Take(4).ToArray()
                                                      : options.Concat(Enumerable.Repeat(string.Empty, 4 - options.Length)).ToArray();
                    }

                    items.Add(new GeneratedExamItem(type, q, a, options, ord, 1m));
                }
            }

            items = Rebalance(items, mcq, tf, sa).Select((it, i) => it with { Order = i + 1, Points = 1m }).ToList();

            return new GeneratedExamResult(title, items);
        }

        private static string BuildPrompt(string courseName,
            IEnumerable<(string ModuleTitle, string OriginalContent)> modules,
            int total, int mcq, int tf, int sa)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"COURSE: {courseName}\n");
            sb.AppendLine("MODULES (original content):");
            foreach (var m in modules) { sb.AppendLine($"### {m.ModuleTitle}\n{m.OriginalContent}\n"); }
            sb.AppendLine($$"""
        TASK:
        Create a single course-wide exam strictly from the content above.

        Constraints:
        - Total questions: {{total}}
        - Distribution: {{mcq}} multiple-choice, {{tf}} true-false, {{sa}} short-answer.
        - Each question is worth exactly 1 point.
        - For multiple-choice, include exactly 4 options with one correct answer.
        - Keep 'order' sequential starting at 1.
        - No external facts; only information from provided modules.

        Return STRICT JSON in this schema (no extra text):
        {
          "title": "string or null",
          "items": [
            {
              "type": "multiple-choice" | "true-false" | "short-answer",
              "questionText": "string (<=1000)",
              "correctAnswer": "string (<=1000)",
              "options": ["A","B","C","D"] | null,
              "order": number
            }
          ]
        }
        """);
            return sb.ToString();
        }

        private static List<GeneratedExamItem> Rebalance(List<GeneratedExamItem> items, int mcq, int tf, int sa)
        {
            var mcqs = items.Where(i => i.Type == "multiple-choice").Take(mcq).ToList();
            var tfs = items.Where(i => i.Type == "true-false").Take(tf).ToList();
            var sas = items.Where(i => i.Type == "short-answer").Take(sa).ToList();

            var need = mcq + tf + sa;
            var combined = new List<GeneratedExamItem>(mcqs.Count + tfs.Count + sas.Count);
            combined.AddRange(mcqs); combined.AddRange(tfs); combined.AddRange(sas);

            if (combined.Count < need)
            {
                var leftovers = items.Except(combined).ToList();
                combined.AddRange(leftovers.Take(need - combined.Count));
            }
            return combined.Take(need).ToList();
        }
    }
}

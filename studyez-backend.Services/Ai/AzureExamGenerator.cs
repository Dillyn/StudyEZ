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

        /// <summary>
        /// // Asynchronously generates an exam from the provided course content and specified question distribution.
        /// </summary>
        public async Task<GeneratedExamResult> GenerateAsync(
            string courseName,
            IEnumerable<(string ModuleTitle, string OriginalContent)> modules,
            int totalQuestions = 20,
            CancellationToken ct = default)
        {
            // Calculate the number of multiple-choice (MCQ), true-false (TF), and short-answer (SA) questions.
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

            // Call the OpenAI API using the rest client to generate the exam content.
            var raw = await _rest.ChatAsync(_opt.ExamModelOrDeployment, body, ct);

            // Parse the raw JSON response to extract the exam title and items.
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var title = root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;

            var items = new List<GeneratedExamItem>();


            // If the "items" property exists and is an array, process each item.
            if (root.TryGetProperty("items", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in arr.EnumerateArray())
                {

                    // Extract the question type, question text, correct answer, and order.
                    var type = el.GetProperty("type").GetString()!;
                    var q = el.GetProperty("questionText").GetString()!;
                    var a = el.GetProperty("correctAnswer").GetString()!;
                    var ord = el.GetProperty("order").GetInt32();

                    // Initialize options array for multiple-choice questions
                    // optional due to MCQ only
                    string[]? options = null;
                    if (type == "multiple-choice" &&
                        el.TryGetProperty("options", out var optEl) &&
                        optEl.ValueKind == JsonValueKind.Array)
                    {
                        // Extract options from the JSON array
                        options = optEl.EnumerateArray()
                                       .Select(x => x.GetString() ?? "")
                                       .ToArray();

                        // Ensure exactly 4 options
                        options = options.Length >= 4
                            ? options.Take(4).ToArray()
                            : options.Concat(Enumerable.Repeat(string.Empty, 4 - options.Length)).ToArray();
                    }

                    // Get module index for each question
                    // moduleIndex (default 1 if missing)
                    int moduleIndex = el.TryGetProperty("moduleIndex", out var miEl) &&
                                      miEl.ValueKind == JsonValueKind.Number
                        ? miEl.GetInt32()
                        : 1;
                    // Create a new GeneratedExamItem and add it to the list.
                    items.Add(new GeneratedExamItem(
                        type,
                        q,
                        a,
                        options,
                        ord,
                        1m,
                        moduleIndex));
                }
            }

            items = Rebalance(items, mcq, tf, sa).Select((it, i) => it with { Order = i + 1 }).ToList();

            return new GeneratedExamResult(title, items);
        }

        private static string BuildPrompt(string courseName,
           IEnumerable<(string ModuleTitle, string OriginalContent)> modules,
           int total, int mcq, int tf, int sa)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"COURSE: {courseName}");
            sb.AppendLine();

            var moduleList = modules.ToList();
            sb.AppendLine("MODULES (original content):");

            for (var i = 0; i < moduleList.Count; i++)
            {
                var m = moduleList[i];
                var idx = i + 1;

                sb.AppendLine($"### MODULE {idx}: {m.ModuleTitle}");
                sb.AppendLine(m.OriginalContent);
                sb.AppendLine();
            }

            // Add task instructions, including the number and distribution of questions.
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
- Each question MUST include a `moduleIndex` field set to the MODULE number (1-based) that best matches that question.

Return STRICT JSON in this schema (no extra text):
{
  "title": "string or null",
  "items": [
    {
      "type": "multiple-choice" | "true-false" | "short-answer",
      "questionText": "string (<=1000)",
      "correctAnswer": "string (<=1000)",
      "options": ["A","B","C","D"] | null,
      "order": number,
      "moduleIndex": number   // 1..N, corresponding to MODULE N above
    }
  ]
}
""");
            return sb.ToString();
        }

        /// <summary>
        /// Rebalance the generated items to match the requested counts per type.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="mcq"></param>
        /// <param name="tf"></param>
        /// <param name="sa"></param>
        /// <returns>List of generated exam items</returns>
        private static List<GeneratedExamItem> Rebalance(List<GeneratedExamItem> items, int mcq, int tf, int sa)
        {
            // Separate items by type: MCQs, TFs, and SAs
            var mcqs = items.Where(i => i.Type == "multiple-choice").Take(mcq).ToList();
            var tfs = items.Where(i => i.Type == "true-false").Take(tf).ToList();
            var sas = items.Where(i => i.Type == "short-answer").Take(sa).ToList();

            // Calculate the total number of items needed.
            var need = mcq + tf + sa;


            var combined = new List<GeneratedExamItem>(mcqs.Count + tfs.Count + sas.Count);
            combined.AddRange(mcqs); combined.AddRange(tfs); combined.AddRange(sas);

            // If there are not enough items, add leftovers to fill the remaining slots.
            if (combined.Count < need)
            {
                var leftovers = items.Except(combined).ToList();
                combined.AddRange(leftovers.Take(need - combined.Count));
            }

            // Return the balanced list, limited to the requested number of items.
            return combined.Take(need).ToList();
        }
    }
}

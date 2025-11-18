using System.Data;
using System.Text.RegularExpressions;
using DbQuestionType = studyez_backend.Core.Constants.Constants.QuestionType;

namespace studyez_backend.Services.Helper
{
    public class Helpers
    {

        public static bool TryMapAiTypeToEnum(string? input, out DbQuestionType t)
        {
            t = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // normalize: keep letters only, lower
            var key = new string(input
                .Where(char.IsLetter)
                .Select(char.ToLowerInvariant)
                .ToArray());

            switch (key)
            {
                case "multiplechoice":
                case "mcq":
                    t = DbQuestionType.MultipleChoice; return true;
                case "truefalse":
                case "tf":
                    t = DbQuestionType.TrueFalse; return true;
                case "shortanswer":
                case "sa":
                    t = DbQuestionType.ShortAnswer; return true;
                default:
                    return false;
            }
        }

        public static string StripMarkdown(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var s = Regex.Replace(input, @"```[\s\S]*?```", " ");         // code blocks
            s = Regex.Replace(s, @"`[^`]*`", " ");                        // inline code
            s = Regex.Replace(s, @"\[(.*?)\]\(.*?\)", "$1");              // [text](url)
            s = Regex.Replace(s, @"^#+\s*", "", RegexOptions.Multiline);  // headings
            s = Regex.Replace(s, @"[*_~>#\-]", " ");                      // bullets/emphasis
            s = Regex.Replace(s, @"\s+", " ");                            // collapse whitespace
            return s.Trim();
        }
    }
}

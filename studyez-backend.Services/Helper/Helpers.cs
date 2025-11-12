using System.Data;
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
    }
}

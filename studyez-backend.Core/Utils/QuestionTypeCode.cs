using studyez_backend.Core.Entities;
using DbQuestionType = studyez_backend.Core.Constants.Constants.QuestionType;

namespace studyez_backend.Core.Utils
{
    /// <summary>
    /// Codec for converting between DbQuestionType enum and wire-format strings.
    /// </summary>
    public static class QuestionTypeCodec
    {
        public static string ToWire(DbQuestionType t) => t switch
        {
            DbQuestionType.MultipleChoice => "multiple-choice",
            DbQuestionType.TrueFalse => "true-false",
            DbQuestionType.ShortAnswer => "short-answer",
            _ => throw new ArgumentOutOfRangeException(nameof(t), t, "Unknown question type")
        };

        public static DbQuestionType FromWire(string s) => (s ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "multiple-choice" => DbQuestionType.MultipleChoice,
            "true-false" => DbQuestionType.TrueFalse,
            "short-answer" => DbQuestionType.ShortAnswer,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown question type")
        };

        // helper
        public static bool IsMultipleChoice(string s) =>
            string.Equals(s, "multiple-choice", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Nice-to-use helpers directly on your entity.
    /// </summary>
    public static class QuestionTypeExtensions
    {
        public static string TypeString(this Question q) => QuestionTypeCodec.ToWire(q.Type);
        public static bool IsMultipleChoice(this Question q) => q.Type == DbQuestionType.MultipleChoice;
        public static bool IsTrueFalse(this Question q) => q.Type == DbQuestionType.TrueFalse;
        public static bool IsShortAnswer(this Question q) => q.Type == DbQuestionType.ShortAnswer;
    }
}

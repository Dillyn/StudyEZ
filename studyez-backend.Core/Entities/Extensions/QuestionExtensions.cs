using DbType = studyez_backend.Core.Constants.Constants.QuestionType;

namespace studyez_backend.Core.Entities.Extensions
{
    public static class QuestionExtensions
    {
        public static string TypeString(this Question q) => q.Type switch
        {
            DbType.MultipleChoice => "multiple-choice",
            DbType.TrueFalse => "true-false",
            DbType.ShortAnswer => "short-answer",
            _ => "short-answer"
        };
    }
}

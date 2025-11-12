using static studyez_backend.Core.Constants.Constants;

namespace studyez_backend.Core.Entities
{
    public sealed class Question : AuditableEntity
    {
        public Guid ModuleId { get; set; }
        public QuestionType Type { get; set; }
        public string QuestionText { get; set; } = default!;
        public string CorrectAnswer { get; set; } = default!;

        public Module Module { get; set; } = default!;
        public ICollection<QuestionOption> Options { get; set; } = [];
        public ICollection<ExamQuestion> ExamQuestions { get; set; } = [];
    }
}

namespace studyez_backend.Core.Entities
{
    public sealed class ExamResultAnswer : AuditableEntity
    {
        public Guid ExamResultId { get; set; }
        public Guid QuestionId { get; set; }
        public string UserAnswer { get; set; } = default!;
        public bool IsCorrect { get; set; }
        public decimal Points { get; set; } = 0m;

        public ExamResult ExamResult { get; set; } = default!;
        public Question Question { get; set; } = default!;
    }
}

namespace studyez_backend.Core.Entities
{
    public sealed class ExamQuestion : AuditableEntity
    {
        public Guid ExamId { get; set; }
        public Guid QuestionId { get; set; }
        public int Order { get; set; } = 0;
        public decimal Points { get; set; } = 1.0m;

        public Exam Exam { get; set; } = default!;
        public Question Question { get; set; } = default!;
    }
}

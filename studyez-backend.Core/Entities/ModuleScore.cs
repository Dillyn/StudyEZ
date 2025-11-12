namespace studyez_backend.Core.Entities
{
    public sealed class ModuleScore : AuditableEntity
    {
        public Guid ExamResultId { get; set; }
        public Guid ModuleId { get; set; }
        public decimal Score { get; set; }
        public int QuestionsCount { get; set; }
        public int CorrectCount { get; set; }

        public ExamResult ExamResult { get; set; } = default!;
        public Module Module { get; set; } = default!;
    }
}

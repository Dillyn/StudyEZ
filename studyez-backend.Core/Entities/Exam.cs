namespace studyez_backend.Core.Entities
{
    public sealed class Exam : AuditableEntity
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        public Course Course { get; set; } = default!;
        public ICollection<ExamQuestion> ExamQuestions { get; set; } = [];
        public ICollection<ExamResult> ExamResults { get; set; } = [];
    }
}

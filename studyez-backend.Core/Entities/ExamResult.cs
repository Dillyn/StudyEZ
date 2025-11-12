namespace studyez_backend.Core.Entities
{
    public sealed class ExamResult : AuditableEntity
    {
        public Guid ExamId { get; set; }
        public Guid UserId { get; set; }
        public decimal OverallScore { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan? TimeTaken { get; set; }

        public Exam Exam { get; set; } = default!;
        public User User { get; set; } = default!;
        public ICollection<ExamResultAnswer> Answers { get; set; } = [];
        public ICollection<ModuleScore> ModuleScores { get; set; } = [];
    }
}

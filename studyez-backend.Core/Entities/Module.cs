namespace studyez_backend.Core.Entities
{
    public sealed class Module : AuditableEntity
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = default!;
        public int Order { get; set; } = 0;
        public string OriginalContent { get; set; } = default!;
        public string? SimplifiedContent { get; set; }
        public bool IsDeleted { get; set; } = false;

        public Course Course { get; set; } = default!;
        public ICollection<Question> Questions { get; set; } = [];
    }

}

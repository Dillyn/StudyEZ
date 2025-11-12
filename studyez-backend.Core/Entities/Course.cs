namespace studyez_backend.Core.Entities
{
    public sealed class Course : AuditableEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; } = false;

        public User User { get; set; } = default!;
        public ICollection<Module> Modules { get; set; } = [];
        public ICollection<Exam> Exams { get; set; } = [];
    }
}

using static studyez_backend.Core.Constants.Constants;

namespace studyez_backend.Core.Entities
{
    public sealed class User : AuditableEntity
    {
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public UserRole Role { get; set; }
        public string? Avatar { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Course> Courses { get; set; } = [];
        public ICollection<ExamResult> ExamResults { get; set; } = [];
    }
}

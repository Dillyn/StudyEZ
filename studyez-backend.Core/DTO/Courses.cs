namespace studyez_backend.Core.DTO
{
    public sealed record CourseDto(Guid Id, Guid UserId, string Name, string Subject, string? Description, DateTime CreatedAt);
    public sealed record CreateCourseCommand(Guid UserId, string Name, string Subject, string? Description);
    public sealed record UpdateCourseCommand(string? Name, string? Subject, string? Description);

    public sealed class CreateCourseRequest
    {
        public Guid UserId { get; init; }
        public string Name { get; init; } = default!;
        public string Subject { get; init; } = default!;
        public string? Description { get; init; }
    }
    public sealed class UpdateCourseRequest
    {
        public string? Name { get; init; }
        public string? Subject { get; init; }
        public string? Description { get; init; }
    }
}

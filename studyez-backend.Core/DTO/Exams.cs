namespace studyez_backend.Core.DTO
{
    public sealed record ExamDto(Guid Id, Guid CourseId, string? Title, DateTime CreatedAt, bool IsActive);
    public sealed record ExamCreateCommand(Guid CourseId, int TotalQuestions = 20);
}

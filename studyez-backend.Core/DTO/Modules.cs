using Microsoft.AspNetCore.Http;

namespace studyez_backend.Core.DTO
{
    public sealed record ModuleDto(Guid Id, Guid CourseId, string Title, int Order, string OriginalContent, string? SimplifiedContent);
    public sealed record CreateModuleCommand(Guid CourseId, string Title, int Order, string OriginalContent);
    public sealed record UploadModuleCommand(Guid CourseId, string Title, int Order, Stream File, string FileName, string ContentType);
    public sealed record UpdateModuleCommand(string? Title, int? Order, string? OriginalContent);

    public sealed class CreateModuleRequest
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = default!;
        public int Order { get; init; } = 0;
        public string OriginalContent { get; init; } = default!;
    }

    public sealed class UpdateModuleRequest
    {
        public string? Title { get; init; }
        public int? Order { get; init; }
        public string? OriginalContent { get; init; }
    }

    public sealed class UploadModuleFileRequest
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = default!;
        public int Order { get; init; } = 0;
        public IFormFile File { get; init; } = default!;
    }
}

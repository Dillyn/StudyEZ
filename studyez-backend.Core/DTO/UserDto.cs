namespace studyez_backend.Core.DTO
{
    public sealed record UserDto(
    Guid Id, string Email, string Name, string Role, string? Avatar,
    DateTime? LastLoginAt, bool IsActive, DateTime CreatedAt);
}

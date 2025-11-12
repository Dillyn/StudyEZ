namespace studyez_backend.Core.DTO
{
    public sealed record QuestionOptionDto(Guid Id, string OptionText, int Order);

    public sealed record QuestionReadDto(
        Guid Id,
        Guid ModuleId,
        string Type,
        string QuestionText,
        string CorrectAnswer,
        IReadOnlyList<QuestionOptionDto> Options);

    public sealed record QuestionCreateDto(
        Guid ModuleId,
        string Type,                 // "multiple-choice" | "true-false" | "short-answer"
        string QuestionText,
        string CorrectAnswer,
        IReadOnlyList<string>? Options // required for MCQ (exactly 4)
    );

    public sealed record QuestionUpdateDto(
        string? Type,
        string? QuestionText,
        string? CorrectAnswer,
        IReadOnlyList<string>? Options // only used for MCQ
    );
}

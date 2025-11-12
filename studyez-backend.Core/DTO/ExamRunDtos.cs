namespace studyez_backend.Core.DTO
{
    public sealed record ExamStartDto(
    Guid ExamId,
    string? Title,
    int TotalQuestions,
    IReadOnlyList<ExamQuestionItemDto> Items);

    public sealed record ExamQuestionItemDto(
        Guid QuestionId,
        string Type,                  // hyphenated
        string QuestionText,
        IReadOnlyList<string>? Options,  // MCQ only
        int Order,
        decimal Points);

    public sealed record SubmitExamRequest(
        Guid ExamId,
        IReadOnlyList<ExamSubmitItem> Answers);

    public sealed record ExamSubmitItem(
        Guid QuestionId,
        string UserAnswer);
}

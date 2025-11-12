namespace studyez_backend.Core.DTO
{
    public sealed record ExamResultSummaryDto(Guid Id, Guid ExamId, Guid UserId, decimal OverallScore, int TotalQuestions, int CorrectAnswers, DateTime CompletedAt);
    public sealed record CreateExamResultCommand(
        Guid ExamId, Guid UserId, decimal OverallScore, int TotalQuestions, int CorrectAnswers, TimeSpan? TimeTaken,
        IReadOnlyList<ExamResultAnswerDto> Answers, IReadOnlyList<ModuleScoreItem> ModuleScores);

    public sealed record ExamResultAnswerDto(Guid QuestionId, string UserAnswer, bool IsCorrect, decimal Points);
    public sealed record ModuleScoreItem(Guid ModuleId, decimal Score, int QuestionsCount, int CorrectCount);
}

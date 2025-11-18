namespace studyez_backend.Core.Interfaces
{
    public sealed record GeneratedExamItem(
    string Type,
    string QuestionText,
    string CorrectAnswer,
    string[]? Options,
    int Order,
    decimal Points,
    int ModuleIndex
);

    public sealed record GeneratedExamResult(string? Title, List<GeneratedExamItem> Items);

    public interface IExamGenerator
    {
        /// <summary>
        /// Generates a course-wide exam from original module content.
        /// Enforces 70% MCQ, 20% True/False, 10% Short Answer. 1 point/question.
        /// </summary>
        Task<GeneratedExamResult> GenerateAsync(
            string courseName,
            IEnumerable<(string ModuleTitle, string OriginalContent)> modules,
            int totalQuestions = 20,
            CancellationToken ct = default);
    }
}

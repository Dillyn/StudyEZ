using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IExamRepository
    {
        /// <summary>
        /// Fetch exam by its unique identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Fetch all exams for a given course
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<Exam>> GetByCourseAsync(Guid courseId, CancellationToken ct);
        /// <summary>
        /// Add a new exam
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(Exam entity, CancellationToken ct);

        // Question write ops
        /// <summary>
        /// Add a new question
        /// </summary>
        /// <param name="q"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddQuestionAsync(Question q, CancellationToken ct);
        /// <summary>
        /// Add a new question option
        /// </summary>
        /// <param name="qo"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddQuestionOptionAsync(QuestionOption qo, CancellationToken ct);
        /// <summary>
        /// Link question to exam
        /// </summary>
        /// <param name="eq"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddExamQuestionAsync(ExamQuestion eq, CancellationToken ct);


        /// <summary>
        /// Get all questions for a given exam, including their options
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<ExamQuestion>> GetExamQuestionsAsync(Guid examId, CancellationToken ct);

        // persist grading artifacts
        /// <summary>
        /// Add exam result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddResultAsync(ExamResult result, CancellationToken ct);
        /// <summary>
        /// Add exam result answers
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddResultAnswersAsync(IEnumerable<ExamResultAnswer> answers, CancellationToken ct);
        /// <summary>
        /// Add module scores
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddModuleScoresAsync(IEnumerable<ModuleScore> scores, CancellationToken ct);

        // Check if results exist for an exam
        /// <summary>
        /// Check if an exam has any results
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> HasResultsAsync(Guid examId, CancellationToken ct);

        // Restrict delete if results exist
        /// <summary>
        /// Try to delete an exam. Fails if results exist.
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> TryDeleteAsync(Guid examId, CancellationToken ct);
    }
}

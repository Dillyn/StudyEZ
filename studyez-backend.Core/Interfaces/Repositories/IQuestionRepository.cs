using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IQuestionRepository
    {
        /// <summary>
        /// Get question by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Question?> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Get questions by module id
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<Question>> GetByModuleAsync(Guid moduleId, CancellationToken ct);
        /// <summary>
        /// Add question
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(Question entity, CancellationToken ct);
        /// <summary>
        /// Update question
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(Question entity, CancellationToken ct);
        /// <summary>
        /// Delete question
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Add question options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddOptionsAsync(IEnumerable<QuestionOption> options, CancellationToken ct);
        /// <summary>
        /// Remove all options for a question
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RemoveOptionsForQuestionAsync(Guid questionId, CancellationToken ct);

        // Helpers for exam run
        /// <summary>
        /// Get questions linked to an exam
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<(Question Q, ExamQuestion Link)>> GetExamQuestionsAsync(Guid examId, CancellationToken ct);

        // Get UserId for ownership checks
        /// <summary>
        /// Get owner UserId by question
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Guid?> GetOwnerUserIdByQuestionAsync(Guid questionId, CancellationToken ct);
        /// <summary>
        /// Get owner UserId by module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Guid?> GetOwnerUserIdByModuleAsync(Guid moduleId, CancellationToken ct);
    }
}

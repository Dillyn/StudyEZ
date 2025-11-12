using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IExamResultRepository
    {
        /// <summary>
        /// Get exam result by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamResult?> GetByIdAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Get exam results by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<ExamResult>> GetByUserAsync(Guid userId, CancellationToken ct);
        /// <summary>
        /// Get exam results by exam id
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<ExamResult>> GetByExamAsync(Guid examId, CancellationToken ct);

        /// <summary>
        /// Add new exam result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(ExamResult result, CancellationToken ct);
        /// <summary>
        /// Add answer to exam result
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAnswerAsync(ExamResultAnswer answer, CancellationToken ct);
        /// <summary>
        /// Add module score
        /// </summary>
        /// <param name="score"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddModuleScoreAsync(ModuleScore score, CancellationToken ct);
    }
}

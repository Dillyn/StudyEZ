using studyez_backend.Core.DTO;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IExamResultService
    {
        /// <summary>
        /// Get Exam Result by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamResultDetailDto> GetAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Get Exam Results by User Id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ExamResultSummaryDto>> GetByUserAsync(Guid userId, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Get Exam Results by Exam Id
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ExamResultSummaryDto>> GetByExamAsync(Guid examId, Guid actorUserId, string actorRole, CancellationToken ct);

        /// <summary>
        /// Create Exam Result
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamResultSummaryDto> CreateAsync(CreateExamResultCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
    }
}

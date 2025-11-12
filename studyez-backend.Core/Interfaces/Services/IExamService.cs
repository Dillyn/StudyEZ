using studyez_backend.Core.DTO;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IExamService
    {
        /// <summary>
        /// Get exam by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamDto> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Get exams by course id
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ExamDto>> GetByCourseAsync(Guid courseId, CancellationToken ct);

        // write operations
        /// <summary>
        /// Generate exam
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamDto> GenerateAsync(ExamCreateCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Deletes an exam
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct);
    }
}

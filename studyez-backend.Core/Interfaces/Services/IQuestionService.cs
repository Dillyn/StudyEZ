using studyez_backend.Core.DTO;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IQuestionService
    {
        /// <summary>
        /// Get question by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<QuestionReadDto?> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Get questions by module id
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<QuestionReadDto>> GetByModuleAsync(Guid moduleId, CancellationToken ct);

        // writes carry actor/actor role
        /// <summary>
        /// Create question
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Guid> CreateAsync(QuestionCreateDto dto, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Update question
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, QuestionUpdateDto dto, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Delete question
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct);
    }
}

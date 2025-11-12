using studyez_backend.Core.DTO;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IModuleService
    {
        //  read operations
        /// <summary>
        /// Fetches a module via its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Getches all modules for a given course
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ModuleDto>> GetByCourseAsync(Guid courseId, CancellationToken ct);

        //  write operations
        /// <summary>
        /// Creates a new module
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> CreateAsync(CreateModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Uploads a module from a file
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> UploadAsync(UploadModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Simplifies a module's content using AI
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> SimplifyAsync(Guid moduleId, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Updates an existing module
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> UpdateAsync(Guid id, UpdateModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Deletes a module (soft or hard)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <param name="hard"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct, bool hard = false);

        /// <summary>
        /// Restores a soft-deleted module.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ModuleDto> RestoreAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct);
    }
}

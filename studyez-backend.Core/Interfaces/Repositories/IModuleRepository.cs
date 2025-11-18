using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IModuleRepository
    {
        // read operations
        /// <summary>
        /// Gets a module by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Module?> GetByIdAsync(Guid id, CancellationToken ct);

        /// <summary>
        /// Gets a module by its ID for update (with row locking).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Module?> GetForUpdateAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets a module by its ID for update (with row locking), including soft-deleted modules.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Module?> GetForUpdateIncludingDeletedAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets all modules for a given course.
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<Module>> GetByCourseAsync(Guid courseId, CancellationToken ct);


        /// <summary>
        /// Gets the owner user ID of a module.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Guid?> GetOwnerUserIdAsync(Guid moduleId, CancellationToken ct);

        // write operations
        /// <summary>
        /// Adds a new module.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(Module entity, CancellationToken ct);
        /// <summary>
        /// Updates an existing module.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(Module entity, CancellationToken ct);
        /// <summary>
        /// Soft-deletes a module.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SoftDeleteAsync(Module entity, CancellationToken ct);
        /// <summary>
        /// Hard-deletes a module.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task HardDeleteAsync(Module entity, CancellationToken ct);

        /// <summary>
        /// Restores a soft-deleted module.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorId"></param>
        /// <param name="when"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RestoreAsync(Guid id, Guid actorId, DateTime when, CancellationToken ct);
    }
}

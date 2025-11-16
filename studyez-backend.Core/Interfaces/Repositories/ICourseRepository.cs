using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface ICourseRepository
    {
        // Reads
        /// <summary>
        /// Gets a course by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Course?> GetByIdAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets a course by its ID for update (with row lock).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Course?> GetForUpdateAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets a course by its ID for update (with row lock), including soft-deleted ones.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Course?> GetForUpdateIncludingDeletedAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets all courses for a given user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<FetchCourseDto>> GetByUserAsync(Guid userId, CancellationToken ct);
        /// <summary>
        /// Checks if a course with the given name exists for the user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct);

        // Writes (no SaveChanges here)

        /// <summary>
        /// Adds a new course.
        /// </summary>
        /// <param name="course"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(Course course, CancellationToken ct);
        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="course"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(Course course, CancellationToken ct);
        /// <summary>
        /// Soft deletes a course.
        /// </summary>
        /// <param name="course"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SoftDeleteAsync(Course course, CancellationToken ct);
        /// <summary>
        /// Hard deletes a course.
        /// </summary>
        /// <param name="course"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task HardDeleteAsync(Course course, CancellationToken ct);
        /// <summary>
        /// Restores a soft-deleted course.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorId"></param>
        /// <param name="when"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task RestoreAsync(Guid id, Guid actorId, DateTime when, CancellationToken ct);
    }
}

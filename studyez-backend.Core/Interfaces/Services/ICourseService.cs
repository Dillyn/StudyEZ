using studyez_backend.Core.DTO;


namespace studyez_backend.Core.Interfaces.Services
{
    public interface ICourseService
    {
        // read operations
        /// <summary>
        /// Gets a course by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CourseDto> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Gets all courses for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<CourseDto>> GetByUserAsync(Guid userId, Guid actorUserId, string actorRole, CancellationToken ct);

        // write operations
        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CourseDto> CreateAsync(CreateCourseCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmd"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CourseDto> UpdateAsync(Guid id, UpdateCourseCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct);
        /// <summary>
        /// Deletes a course (soft or hard).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <param name="hardDelete"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct, bool hardDelete = false);

        /// <summary>
        /// Restores a soft-deleted course.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="actorUserId"></param>
        /// <param name="actorRole"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CourseDto> RestoreAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct);
    }
}

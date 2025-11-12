using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// fetches a user by its id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// fetches a user by its email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        /// <summary>
        /// adds a new user to the database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(User user, CancellationToken ct);
        /// <summary>
        /// updates an existing user in the database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(User user, CancellationToken ct);
    }
}

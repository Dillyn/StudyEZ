using studyez_backend.Core.DTO;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<UserDto> GetAsync(Guid id, CancellationToken ct);
        /// <summary>
        /// Upsert user from Google info
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="avatarUrl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<UserDto> UpsertFromGoogleAsync(string email, string name, string? avatarUrl, CancellationToken ct);
    }
}

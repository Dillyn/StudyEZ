using Microsoft.Extensions.Logging;
using studyez_backend.Core.Constants;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;

namespace studyez_backend.Services.Services
{
    public sealed class UserService(IUserRepository users, IUnitOfWork uow, ILogger<UserService> log) : IUserService
    {
        public async Task<UserDto> GetAsync(Guid id, CancellationToken ct)
        {
            var u = await users.GetByIdAsync(id, ct) ?? throw new UserNotFoundException(id);
            return ToDto(u);
        }

        public async Task<UserDto> UpsertFromGoogleAsync(string email, string name, string? avatarUrl, CancellationToken ct)
        {
            var existing = await users.GetByEmailAsync(email, ct);

            if (existing is null)
            {
                var now = DateTime.UtcNow;
                var u = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email.Trim(),
                    Name = string.IsNullOrWhiteSpace(name) ? email : name.Trim(),
                    PasswordHash = string.Empty,
                    Role = Constants.UserRole.Free,
                    Avatar = avatarUrl,
                    LastLoginAt = now,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = Guid.Empty
                };
                await users.AddAsync(u, ct);
                await uow.SaveChangesAsync(ct);
                log.LogInformation("Created new user from Google: {Email}", email);
                return ToDto(u);
            }
            else
            {
                existing.Name = string.IsNullOrWhiteSpace(name) ? existing.Name : name.Trim();
                existing.Avatar = avatarUrl ?? existing.Avatar;
                existing.LastLoginAt = DateTime.UtcNow;
                await users.UpdateAsync(existing, ct);
                await uow.SaveChangesAsync(ct);
                log.LogInformation("Updated login timestamp for user: {Email}", email);
                return ToDto(existing);
            }
        }

        private static UserDto ToDto(User u)
            => new(u.Id, u.Email, u.Name, u.Role.ToString(), u.Avatar, u.LastLoginAt, u.IsActive, u.CreatedAt);
    }
}

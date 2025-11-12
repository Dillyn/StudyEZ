using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class UserRepository(ApplicationDbContext db) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
            => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
            => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);

        public Task AddAsync(User user, CancellationToken ct)
        {
            db.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken ct)
        {
            db.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}

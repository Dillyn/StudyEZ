using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
    {
        private readonly ApplicationDbContext _db = db;
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}

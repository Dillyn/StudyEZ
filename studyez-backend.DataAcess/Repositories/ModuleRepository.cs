using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class ModuleRepository(ApplicationDbContext db) : IModuleRepository
    {
        public Task<Module?> GetByIdAsync(Guid id, CancellationToken ct) =>
            db.Modules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Module?> GetForUpdateAsync(Guid id, CancellationToken ct) =>
            db.Modules.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Module?> GetForUpdateIncludingDeletedAsync(Guid id, CancellationToken ct) =>
            db.Modules.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<Module>> GetByCourseAsync(Guid courseId, CancellationToken ct) =>
            db.Modules.AsNoTracking()
                      .Where(x => x.CourseId == courseId)
                      .OrderBy(x => x.CreatedAt)
                      .ToListAsync(ct);

        public Task AddAsync(Module entity, CancellationToken ct)
        {
            db.Modules.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Module entity, CancellationToken ct) => Task.CompletedTask;

        public Task SoftDeleteAsync(Module entity, CancellationToken ct)
        {
            entity.IsDeleted = true;
            return Task.CompletedTask;
        }

        public Task HardDeleteAsync(Module entity, CancellationToken ct)
        {
            db.Modules.Remove(entity);
            return Task.CompletedTask;
        }

        public Task RestoreAsync(Guid id, Guid actorId, DateTime when, CancellationToken ct) =>
            db.Modules
              .IgnoreQueryFilters()
              .Where(x => x.Id == id)
              .ExecuteUpdateAsync(u => u
                  .SetProperty(m => m.IsDeleted, false)
                  .SetProperty(m => m.UpdatedAt, when)
                  .SetProperty(m => m.UpdatedBy, actorId), ct);

        public Task<Guid?> GetOwnerUserIdAsync(Guid moduleId, CancellationToken ct) =>
            db.Modules
              .AsNoTracking()
              .Where(m => m.Id == moduleId)
              .Select(m => (Guid?)m.Course.UserId)
              .FirstOrDefaultAsync(ct);
    }
}

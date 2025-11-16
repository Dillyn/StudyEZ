using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public class CourseRepository(ApplicationDbContext db) : ICourseRepository
    {
        public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct)
            => db.Courses
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Course?> GetForUpdateAsync(Guid id, CancellationToken ct)
            => db.Courses
                 .FirstOrDefaultAsync(x => x.Id == id, ct); // tracked

        public Task<Course?> GetForUpdateIncludingDeletedAsync(Guid id, CancellationToken ct)
            => db.Courses
                 .IgnoreQueryFilters()
                 .FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<FetchCourseDto>> GetByUserAsync(Guid userId, CancellationToken ct)
            => db.Courses
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new FetchCourseDto(
                c.Id,
                c.UserId,
                c.Name,
                c.Subject,
                c.Description,
                c.CreatedAt,
                c.Modules.Count(m => !m.IsDeleted) // <-- SQL COUNT(*)
            ))
            .ToListAsync(ct);

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
            => db.Courses
                 .AsNoTracking()
                 .AnyAsync(x => x.UserId == userId && x.Name == name && !x.IsDeleted, ct);

        public Task AddAsync(Course course, CancellationToken ct)
        {
            db.Courses.Add(course);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Course course, CancellationToken ct)
        {
            // TODO: EF Core tracks entities automatically, so this method is currently a no-op.
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(Course course, CancellationToken ct)
        {
            course.IsDeleted = true;
            return Task.CompletedTask;
        }

        public async Task RestoreAsync(Guid id, Guid actorId, DateTime when, CancellationToken ct)
        {
            var entity = await db.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity is null) return;

            entity.IsDeleted = false;
            entity.UpdatedAt = when;
            entity.UpdatedBy = actorId;
        }

        public Task HardDeleteAsync(Course course, CancellationToken ct)
        {
            db.Courses.Remove(course);
            return Task.CompletedTask;
        }
    }
}

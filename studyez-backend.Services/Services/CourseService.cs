using Microsoft.Extensions.Logging;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security;

namespace studyez_backend.Services.Services
{
    public sealed class CourseService(
    ICourseRepository courses,
    IUnitOfWork uow,
    ILogger<CourseService> log) : ICourseService
    {
        private readonly ICourseRepository _courses = courses;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<CourseService> _log = log;

        public async Task<CourseDto> GetAsync(Guid id, CancellationToken ct)
        {
            var c = await _courses.GetByIdAsync(id, ct) ?? throw new CourseNotFoundException(id);
            return ToDto(c);
        }

        public async Task<IReadOnlyList<CourseDto>> GetByUserAsync(Guid userId, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);
            if (!isAdmin && userId != actorUserId)
                throw new ForbiddenException("You can only view your own courses.");

            var list = await _courses.GetByUserAsync(userId, ct);
            return list.Select(ToDto).ToList();
        }


        public async Task<CourseDto> CreateAsync(CreateCourseCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);

            if (!isAdmin && cmd.UserId != actorUserId)
                throw new ForbiddenException("Cannot create courses for other users.");

            if (string.IsNullOrWhiteSpace(cmd.Name))
                throw new ValidationException("Course name is required.");

            if (await _courses.ExistsByNameAsync(cmd.UserId, cmd.Name.Trim(), ct))
                throw new ConflictException($"A course named '{cmd.Name}' already exists for this user.");

            var now = DateTime.UtcNow;

            var entity = new Course
            {
                Id = Guid.NewGuid(),
                UserId = cmd.UserId,
                Name = cmd.Name.Trim(),
                Subject = cmd.Subject.Trim(),
                Description = cmd.Description,
                CreatedAt = now,
                CreatedBy = actorUserId,
                UpdatedAt = null,
                UpdatedBy = null,
                IsDeleted = false
            };

            await _courses.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _log.LogInformation("Course {CourseId} created by {ActorId}", entity.Id, actorUserId);
            return ToDto(entity);
        }

        public async Task<CourseDto> UpdateAsync(Guid id, UpdateCourseCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);
            var c = await _courses.GetForUpdateAsync(id, ct) ?? throw new CourseNotFoundException(id);

            OwnershipGuard.EnsureOwnerOrAdmin(c.UserId, actorUserId, isAdmin);

            if (!string.IsNullOrWhiteSpace(cmd.Name))
            {
                var newName = cmd.Name.Trim();
                if (!newName.Equals(c.Name, StringComparison.Ordinal) &&
                    await _courses.ExistsByNameAsync(c.UserId, newName, ct))
                    throw new ConflictException($"A course named '{newName}' already exists for this user.");

                c.Name = newName;
            }

            if (!string.IsNullOrWhiteSpace(cmd.Subject))
                c.Subject = cmd.Subject.Trim();

            if (cmd.Description is not null)
                c.Description = cmd.Description;

            c.UpdatedAt = DateTime.UtcNow;
            c.UpdatedBy = actorUserId;

            await _courses.UpdateAsync(c, ct);
            await _uow.SaveChangesAsync(ct);

            _log.LogInformation("Course {CourseId} updated by {ActorId}", id, actorUserId);
            return ToDto(c);
        }

        public async Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct, bool hardDelete = false)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);
            var c = await _courses.GetForUpdateAsync(id, ct) ?? throw new CourseNotFoundException(id);

            OwnershipGuard.EnsureOwnerOrAdmin(c.UserId, actorUserId, isAdmin);

            if (hardDelete)
            {
                await _courses.HardDeleteAsync(c, ct);
                await _uow.SaveChangesAsync(ct);
                _log.LogWarning("Course {CourseId} HARD-deleted by {ActorId}", id, actorUserId);
                return;
            }

            await _courses.SoftDeleteAsync(c, ct);
            c.UpdatedAt = DateTime.UtcNow;
            c.UpdatedBy = actorUserId;

            await _uow.SaveChangesAsync(ct);
            _log.LogInformation("Course {CourseId} soft-deleted by {ActorId}", id, actorUserId);
        }

        public async Task<CourseDto> RestoreAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);
            var existing = await _courses.GetForUpdateIncludingDeletedAsync(id, ct) ?? throw new CourseNotFoundException(id);

            OwnershipGuard.EnsureOwnerOrAdmin(existing.UserId, actorUserId, isAdmin);

            var now = DateTime.UtcNow;

            await _courses.RestoreAsync(id, actorUserId, now, ct);
            await _uow.SaveChangesAsync(ct);


            var restored = await _courses.GetByIdAsync(id, ct) ?? existing;
            _log.LogInformation("Course {CourseId} restored by {ActorId}", id, actorUserId);
            return ToDto(restored);
        }

        private static CourseDto ToDto(Course c)
            => new(c.Id, c.UserId, c.Name, c.Subject, c.Description, c.CreatedAt);
    }
}

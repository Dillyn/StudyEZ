using Microsoft.Extensions.Logging;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security;

namespace studyez_backend.Services.Services
{
    /// <summary>
    /// Module application service:
    /// - Validates & enforces business rules
    /// - Writes audit columns (CreatedBy/UpdatedBy, CreatedAt/UpdatedAt)
    /// - Coordinates with AI client for simplification
    /// - Throws AppException subclasses (mapped to HTTP by middleware)
    /// </summary>
    public sealed class ModuleService : IModuleService
    {
        private readonly IModuleRepository _modules;
        private readonly ICourseRepository _courses;
        private readonly IAiClient _ai;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ModuleService> _log;


        public ModuleService(IModuleRepository modules, ICourseRepository courses, IAiClient ai, IUnitOfWork uow, ILogger<ModuleService> log)
        {
            _modules = modules;
            _courses = courses;
            _ai = ai;
            _uow = uow;
            _log = log;
        }

        private static ModuleDto ToDto(Module m)
            => new(m.Id, m.CourseId, m.Title, m.Order, m.OriginalContent, m.SimplifiedContent);

        public async Task<ModuleDto> GetAsync(Guid id, CancellationToken ct)
        {
            var m = await _modules.GetByIdAsync(id, ct)
                     ?? throw new ModuleNotFoundException(id);

            return ToDto(m);
        }

        public async Task<IReadOnlyList<ModuleDto>> GetByCourseAsync(Guid courseId, CancellationToken ct)
        {
            var list = await _modules.GetByCourseAsync(courseId, ct);
            return list.Select(ToDto).ToList();
        }

        public async Task<ModuleDto> CreateAsync(CreateModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);

            var course = await _courses.GetByIdAsync(cmd.CourseId, ct)
                         ?? throw new CourseNotFoundException(cmd.CourseId);

            OwnershipGuard.EnsureOwnerOrAdmin(course.UserId, actorUserId, isAdmin);

            if (string.IsNullOrWhiteSpace(cmd.Title))
                throw new ValidationException("Title is required.");

            var now = DateTime.UtcNow;

            var entity = new Module
            {
                Id = Guid.NewGuid(),
                CourseId = cmd.CourseId,
                Title = cmd.Title.Trim(),
                Order = cmd.Order,
                OriginalContent = cmd.OriginalContent,
                CreatedAt = now,
                CreatedBy = actorUserId,
                IsDeleted = false
            };

            await _modules.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _log.LogInformation("Module {ModuleId} created by {Actor}", entity.Id, actorUserId);
            return ToDto(entity);
        }

        public async Task<ModuleDto> UploadAsync(UploadModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);
            var course = await _courses.GetByIdAsync(cmd.CourseId, ct)
                         ?? throw new CourseNotFoundException(cmd.CourseId);

            OwnershipGuard.EnsureOwnerOrAdmin(course.UserId, actorUserId, isAdmin);

            if (cmd.File is null || cmd.File.Length == 0)
                throw new ValidationException("File content is required.");

            if (string.IsNullOrWhiteSpace(cmd.Title))
                throw new ValidationException("File name is required.");

            using var reader = new StreamReader(cmd.File, leaveOpen: false);
#if NET9_0_OR_GREATER
            var text = await reader.ReadToEndAsync(ct);
#else
            var text = await reader.ReadToEndAsync();
#endif


            var now = DateTime.UtcNow;

            var entity = new Module
            {
                Id = Guid.NewGuid(),
                CourseId = cmd.CourseId,
                Title = cmd.Title.Trim(),
                Order = cmd.Order,
                OriginalContent = text,
                CreatedAt = now,
                CreatedBy = actorUserId,
                IsDeleted = false
            };

            await _modules.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            _log.LogInformation("Module {ModuleId} uploaded by {Actor}", entity.Id, actorUserId);
            return ToDto(entity);
        }

        public async Task<ModuleDto> SimplifyAsync(Guid moduleId, Guid actorUserId, string actorRole, CancellationToken ct)
        {

            var isAdmin = RoleHelper.IsAdmin(actorRole);

            var ownerId = await _modules.GetOwnerUserIdAsync(moduleId, ct)
                          ?? throw new ModuleNotFoundException(moduleId);

            // TODO - above lines commented out for testing purposes - will replace actorUserId with ownerId once testing is complete
            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            var m = await _modules.GetForUpdateAsync(moduleId, ct)
                    ?? throw new ModuleNotFoundException(moduleId);

            if (string.IsNullOrWhiteSpace(m.OriginalContent))
                throw new ValidationException("Module has no original content to simplify.");

            var simplified = await _ai.SimplifyAsync(m.OriginalContent, ct);

            m.SimplifiedContent = simplified;
            m.UpdatedAt = DateTime.UtcNow;
            m.UpdatedBy = actorUserId;

            await _uow.SaveChangesAsync(ct);
            _log.LogInformation("Module {ModuleId} simplified by {Actor}", moduleId, actorUserId);

            return ToDto(m);
        }

        public async Task<ModuleDto> UpdateAsync(Guid id, UpdateModuleCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);

            var ownerId = await _modules.GetOwnerUserIdAsync(id, ct)
                          ?? throw new ModuleNotFoundException(id);

            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            var m = await _modules.GetForUpdateAsync(id, ct)
                    ?? throw new ModuleNotFoundException(id);

            if (cmd.Title is not null)
            {
                var title = cmd.Title.Trim();
                if (title.Length == 0) throw new ValidationException("Title cannot be empty.");
                m.Title = title;
            }

            if (cmd.Order is not null) m.Order = cmd.Order.Value;
            if (cmd.OriginalContent is not null) m.OriginalContent = cmd.OriginalContent;

            m.UpdatedAt = DateTime.UtcNow;
            m.UpdatedBy = actorUserId;

            await _modules.UpdateAsync(m, ct);
            await _uow.SaveChangesAsync(ct);

            _log.LogInformation("Module {ModuleId} updated by {Actor}", id, actorUserId);
            return ToDto(m);
        }

        /// <summary>
        /// Soft delete by default. Pass hard=true to physically remove.
        /// </summary>
        public async Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct, bool hard = false)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);

            var ownerId = await _modules.GetOwnerUserIdAsync(id, ct)
                          ?? throw new ModuleNotFoundException(id);
            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            var m = await _modules.GetForUpdateIncludingDeletedAsync(id, ct) ?? throw new ModuleNotFoundException(id);

            if (hard)
            {
                await _modules.HardDeleteAsync(m, ct);
                await _uow.SaveChangesAsync(ct);
                _log.LogWarning("Module {ModuleId} HARD-deleted by {Actor}", id, actorUserId);
                return;
            }

            await _modules.SoftDeleteAsync(m, ct);
            m.UpdatedAt = DateTime.UtcNow;
            m.UpdatedBy = actorUserId;

            await _uow.SaveChangesAsync(ct);
            _log.LogInformation("Module {ModuleId} soft-deleted by {Actor}", id, actorUserId);
        }

        /// <summary>
        /// Restore a soft-deleted module (bypasses global filters).
        /// </summary>
        public async Task<ModuleDto> RestoreAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var isAdmin = RoleHelper.IsAdmin(actorRole);

            var ownerId = await _modules.GetOwnerUserIdAsync(id, ct)
                          ?? throw new ModuleNotFoundException(id);

            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);


            await _modules.RestoreAsync(id, actorUserId, DateTime.UtcNow, ct);
            await _uow.SaveChangesAsync(ct);

            var restored = await _modules.GetByIdAsync(id, ct)
                           ?? throw new ModuleNotFoundException(id);

            _log.LogInformation("Module {ModuleId} restored by {Actor}", id, actorUserId);
            return ToDto(restored);
        }
    }
}

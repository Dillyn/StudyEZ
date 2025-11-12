using studyez_backend.Core.Constants;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security;
using studyez_backend.Core.Utils;

namespace studyez_backend.Services.Services
{
    public sealed class QuestionService(
    IQuestionRepository questions,
    IUnitOfWork uow) : IQuestionService
    {
        private readonly IQuestionRepository _questions = questions;
        private readonly IUnitOfWork _uow = uow;

        public async Task<QuestionReadDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var q = await _questions.GetAsync(id, ct) ?? throw new QuestionNotFoundException(id);
            return q is null ? null : Map(q);
        }

        public async Task<IReadOnlyList<QuestionReadDto>> GetByModuleAsync(Guid moduleId, CancellationToken ct)
        {
            var list = await _questions.GetByModuleAsync(moduleId, ct);
            return list.Select(Map).ToList();
        }

        public async Task<Guid> CreateAsync(QuestionCreateDto dto, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var ownerId = await _questions.GetOwnerUserIdByModuleAsync(dto.ModuleId, ct)
                          ?? throw new ModuleNotFoundException(dto.ModuleId);

            var isAdmin = RoleHelper.IsAdmin(actorRole);
            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            var qType = QuestionTypeCodec.FromWire(dto.Type);

            if (qType == Constants.QuestionType.MultipleChoice)
            {
                if (dto.Options is null || dto.Options.Count != 4)
                    throw new ValidationException("Options (exactly 4) are required for multiple-choice.");
            }

            var q = new Question
            {
                Id = Guid.NewGuid(),
                ModuleId = dto.ModuleId,
                Type = qType,
                QuestionText = dto.QuestionText.Trim(),
                CorrectAnswer = dto.CorrectAnswer.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorUserId
            };

            _questions.AddAsync(q, ct); // tracked
            if (qType == Constants.QuestionType.MultipleChoice)
            {
                var opts = dto.Options!.Select((t, i) => new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = q.Id,
                    OptionText = t,
                    Order = i + 1,
                    CreatedAt = DateTime.UtcNow
                });
                await _questions.AddOptionsAsync(opts, ct);
            }

            await _uow.SaveChangesAsync(ct);
            return q.Id;
        }

        public async Task UpdateAsync(Guid id, QuestionUpdateDto dto, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var ownerId = await _questions.GetOwnerUserIdByQuestionAsync(id, ct)
                          ?? throw new QuestionNotFoundException(id);

            var isAdmin = RoleHelper.IsAdmin(actorRole);
            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            var q = await _questions.GetAsync(id, ct) ?? throw new QuestionNotFoundException(id);

            var qType = QuestionTypeCodec.FromWire(dto.Type);
            q.Type = qType;
            q.QuestionText = dto.QuestionText.Trim();
            q.CorrectAnswer = dto.CorrectAnswer.Trim();
            q.UpdatedAt = DateTime.UtcNow;
            q.UpdatedBy = actorUserId;

            await _questions.UpdateAsync(q, ct);

            await _questions.RemoveOptionsForQuestionAsync(q.Id, ct);
            if (qType == Constants.QuestionType.MultipleChoice)
            {
                if (dto.Options is null || dto.Options.Count != 4)
                    throw new ValidationException("Options (exactly 4) are required for multiple-choice.");

                var opts = dto.Options.Select((t, i) => new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = q.Id,
                    OptionText = t,
                    Order = i + 1,
                    CreatedAt = DateTime.UtcNow
                });
                await _questions.AddOptionsAsync(opts, ct);
            }

            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var ownerId = await _questions.GetOwnerUserIdByQuestionAsync(id, ct)
                          ?? throw new QuestionNotFoundException(id);

            var isAdmin = RoleHelper.IsAdmin(actorRole);
            OwnershipGuard.EnsureOwnerOrAdmin(ownerId, actorUserId, isAdmin);

            await _questions.DeleteAsync(id, ct);
            await _uow.SaveChangesAsync(ct);
        }

        private static QuestionReadDto Map(Question q) =>
            new(
                q.Id,
                q.ModuleId,
                q.TypeString(),
                q.QuestionText,
                q.CorrectAnswer,
                q.Options?.OrderBy(o => o.Order)
                    .Select(o => new QuestionOptionDto(o.Id, o.OptionText, o.Order))
                    .ToList() ?? new List<QuestionOptionDto>());
    }
}

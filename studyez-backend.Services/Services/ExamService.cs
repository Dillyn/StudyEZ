using studyez_backend.Core.Constants;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security;
using studyez_backend.Services.Helper;

namespace studyez_backend.Services.Services
{
    public sealed class ExamService : IExamService
    {
        private readonly IExamRepository _exams;
        private readonly ICourseRepository _courses;
        private readonly IModuleRepository _modules;
        private readonly IExamGenerator _gen;
        private readonly IUnitOfWork _uow;

        public ExamService(IExamRepository exams, ICourseRepository courses, IModuleRepository modules,
                           IExamGenerator gen, IUnitOfWork uow)
        {
            _exams = exams;
            _courses = courses;
            _modules = modules;
            _gen = gen;
            _uow = uow;
        }

        public async Task<ExamDto> GetAsync(Guid id, CancellationToken ct)
        => ToDto(await _exams.GetByIdAsync(id, ct) ?? throw new ExamNotFoundException(id));

        public async Task<IReadOnlyList<ExamDto>> GetByCourseAsync(Guid courseId, CancellationToken ct)
            => (await _exams.GetByCourseAsync(courseId, ct)).Select(ToDto).ToList();

        public async Task<ExamDto> GenerateAsync(ExamCreateCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var course = await _courses.GetByIdAsync(cmd.CourseId, ct) ?? throw new CourseNotFoundException(cmd.CourseId);

            var isAdmin = RoleHelper.IsAdmin(actorRole);
            OwnershipGuard.EnsureOwnerOrAdmin(course.UserId, actorUserId, isAdmin);

            var mods = await _modules.GetByCourseAsync(course.Id, ct);
            if (mods.Count == 0) throw new InvalidExamGenerationException("Course has no modules.");

            var result = await _gen.GenerateAsync(
                course.Name,
                mods.Select(m => (m.Title, m.OriginalContent ?? string.Empty)),
                cmd.TotalQuestions,
                ct);

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = string.IsNullOrWhiteSpace(result.Title) ? $"{course.Name} — Exam" : result.Title!,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorUserId,
                IsActive = true
            };
            await _exams.AddAsync(exam, ct);

            int order = 1;
            foreach (var item in result.Items.OrderBy(i => i.Order))
            {
                if (!Helpers.TryMapAiTypeToEnum(item.Type, out var qType))
                    throw new ValidationException($"Unknown question type from AI: '{item.Type}'");

                var q = new Question
                {
                    Id = Guid.NewGuid(),
                    ModuleId = mods.First().Id,
                    Type = qType,
                    QuestionText = item.QuestionText.Trim(),
                    CorrectAnswer = item.CorrectAnswer.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actorUserId
                };
                await _exams.AddQuestionAsync(q, ct);

                if (qType == Constants.QuestionType.MultipleChoice && item.Options is { Length: > 0 })
                {
                    var options = item.Options.Length >= 4
                        ? item.Options.Take(4)
                        : item.Options.Concat(Enumerable.Repeat(string.Empty, 4 - item.Options.Length));

                    int optOrder = 1;
                    foreach (var op in options)
                    {
                        await _exams.AddQuestionOptionAsync(new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = q.Id,
                            OptionText = op,
                            Order = optOrder++,
                            CreatedAt = DateTime.UtcNow
                        }, ct);
                    }
                }

                await _exams.AddExamQuestionAsync(new ExamQuestion
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    QuestionId = q.Id,
                    Order = order++,
                    Points = 1m,
                    CreatedAt = DateTime.UtcNow
                }, ct);
            }

            await _uow.SaveChangesAsync(ct);
            return ToDto(exam);
        }

        public async Task DeleteAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var exam = await _exams.GetByIdAsync(id, ct) ?? throw new ExamNotFoundException(id);

            // ownership via course owner
            var course = await _courses.GetByIdAsync(exam.CourseId, ct) ?? throw new CourseNotFoundException(exam.CourseId);
            OwnershipGuard.EnsureOwnerOrAdmin(course.UserId, actorUserId, RoleHelper.IsAdmin(actorRole));

            if (!await _exams.TryDeleteAsync(id, ct))
                throw new ConflictException("Exam has results; cannot delete.");

            await _uow.SaveChangesAsync(ct);
        }

        private static ExamDto ToDto(Exam e) => new(e.Id, e.CourseId, e.Title, e.CreatedAt, e.IsActive);

    }

}

using studyez_backend.Core.DTO;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security;

namespace studyez_backend.Services.Services.ExamResults
{
    public sealed class ExamResultService : IExamResultService
    {
        private readonly IExamRepository _exams;
        private readonly IExamResultRepository _results;
        private readonly IUnitOfWork _uow;

        public ExamResultService(IExamRepository exams, IExamResultRepository results, IUnitOfWork uow)
        {
            _exams = exams; _results = results; _uow = uow;
        }

        public async Task<ExamResultSummaryDto> GetAsync(Guid id, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            var r = await _results.GetByIdAsync(id, ct) ?? throw new ExamResultNotFoundException(id);
            if (!RoleHelper.IsAdmin(actorRole) && r.UserId != actorUserId) throw new ForbiddenException("You can only view your own results.");
            return ToDto(r);
        }

        public async Task<IReadOnlyList<ExamResultSummaryDto>> GetByUserAsync(Guid userId, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            // Only admin can query arbitrary users
            if (!RoleHelper.IsAdmin(actorRole) && userId != actorUserId) throw new ForbiddenException("Not allowed.");
            return (await _results.GetByUserAsync(userId, ct)).Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<ExamResultSummaryDto>> GetByExamAsync(Guid examId, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            // Anyone can list their own results; admin can view all.
            var list = await _results.GetByExamAsync(examId, ct);
            if (RoleHelper.IsAdmin(actorRole)) return list.Select(ToDto).ToList();
            return list.Where(r => r.UserId == actorUserId).Select(ToDto).ToList();
        }

        public async Task<ExamResultSummaryDto> CreateAsync(CreateExamResultCommand cmd, Guid actorUserId, string actorRole, CancellationToken ct)
        {
            if (!RoleHelper.IsAdmin(actorRole) && cmd.UserId != actorUserId)
                throw new ForbiddenException("Cannot create results for other users.");

            _ = await _exams.GetByIdAsync(cmd.ExamId, ct) ?? throw new ExamNotFoundException(cmd.ExamId);

            var r = new Core.Entities.ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = cmd.ExamId,
                UserId = cmd.UserId,
                OverallScore = cmd.OverallScore,
                TotalQuestions = cmd.TotalQuestions,
                CorrectAnswers = cmd.CorrectAnswers,
                CompletedAt = DateTime.UtcNow,
                TimeTaken = cmd.TimeTaken
            };
            await _results.AddAsync(r, ct);

            foreach (var a in cmd.Answers)
            {
                await _results.AddAnswerAsync(new Core.Entities.ExamResultAnswer
                {
                    Id = Guid.NewGuid(),
                    ExamResultId = r.Id,
                    QuestionId = a.QuestionId,
                    UserAnswer = a.UserAnswer,
                    IsCorrect = a.IsCorrect,
                    Points = a.Points
                }, ct);
            }

            foreach (var m in cmd.ModuleScores)
            {
                await _results.AddModuleScoreAsync(new Core.Entities.ModuleScore
                {
                    Id = Guid.NewGuid(),
                    ExamResultId = r.Id,
                    ModuleId = m.ModuleId,
                    Score = m.Score,
                    QuestionsCount = m.QuestionsCount,
                    CorrectCount = m.CorrectCount
                }, ct);
            }

            await _uow.SaveChangesAsync(ct);
            return ToDto(r);
        }

        private static ExamResultSummaryDto ToDto(Core.Entities.ExamResult r)
            => new(r.Id, r.ExamId, r.UserId, r.OverallScore, r.TotalQuestions, r.CorrectAnswers, r.CompletedAt);
    }
}

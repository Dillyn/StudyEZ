using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class ExamResultRepository(ApplicationDbContext db) : IExamResultRepository
    {
        public Task<ExamResult?> GetByIdAsync(Guid id, CancellationToken ct)
            => db.ExamResults
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        public Task<List<ExamResult>> GetByUserAsync(Guid userId, CancellationToken ct)
            => db.ExamResults.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync(ct);

        public Task<List<ExamResult>> GetByExamAsync(Guid examId, CancellationToken ct)
            => db.ExamResults.AsNoTracking().Where(x => x.ExamId == examId).OrderByDescending(x => x.CompletedAt).ToListAsync(ct);

        public Task AddAsync(ExamResult result, CancellationToken ct)
        {
            db.ExamResults.Add(result);
            return Task.CompletedTask;
        }

        public Task AddAnswerAsync(ExamResultAnswer answer, CancellationToken ct)
        {
            db.ExamResultAnswers.Add(answer);
            return Task.CompletedTask;
        }

        public Task AddModuleScoreAsync(ModuleScore score, CancellationToken ct)
        {
            db.ModuleScores.Add(score);
            return Task.CompletedTask;
        }
    }
}

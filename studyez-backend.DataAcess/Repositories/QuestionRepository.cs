using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class QuestionRepository(ApplicationDbContext db) : IQuestionRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Question?> GetAsync(Guid id, CancellationToken ct) =>
            await _db.Questions
                .Include(q => q.Options.OrderBy(o => o.Order))
                .FirstOrDefaultAsync(q => q.Id == id, ct);

        public async Task<IReadOnlyList<Question>> GetByModuleAsync(Guid moduleId, CancellationToken ct) =>
            await _db.Questions.AsNoTracking()
                .Where(q => q.ModuleId == moduleId)
                .Include(q => q.Options.OrderBy(o => o.Order))
                .OrderBy(q => q.CreatedAt)
                .ToListAsync(ct);

        public Task AddAsync(Question entity, CancellationToken ct)
        {
            _db.Questions.Add(entity);
            return Task.CompletedTask;

        }

        public Task UpdateAsync(Question entity, CancellationToken ct)
        {
            _db.Questions.Update(entity);
            return Task.CompletedTask;

        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (q != null)
            {
                _db.Questions.Remove(q);
            }
        }

        public async Task AddOptionsAsync(IEnumerable<QuestionOption> options, CancellationToken ct)
        {
            await _db.QuestionOptions.AddRangeAsync(options, ct);
        }

        public async Task RemoveOptionsForQuestionAsync(Guid questionId, CancellationToken ct)
        {
            var toRemove = await _db.QuestionOptions
                .Where(o => o.QuestionId == questionId)
                .ToListAsync(ct);
            _db.QuestionOptions.RemoveRange(toRemove);
        }

        public async Task<Guid?> GetOwnerUserIdByQuestionAsync(Guid questionId, CancellationToken ct)
        => await db.Questions
            .Where(q => q.Id == questionId)
            .Select(q => q.Module.Course.UserId)
            .FirstOrDefaultAsync(ct) is var uid && uid != Guid.Empty ? uid : null;

        public async Task<Guid?> GetOwnerUserIdByModuleAsync(Guid moduleId, CancellationToken ct)
            => await _db.Modules
                .Where(m => m.Id == moduleId)
                .Select(m => m.Course.UserId)
                .FirstOrDefaultAsync(ct) is var uid && uid != Guid.Empty ? uid : null;

        public async Task<IReadOnlyList<(Question Q, ExamQuestion Link)>> GetExamQuestionsAsync(Guid examId, CancellationToken ct)
        {
            var list = await _db.ExamQuestions
                .Where(eq => eq.ExamId == examId)
                .OrderBy(eq => eq.Order)
                .Include(eq => eq.Question!)
                    .ThenInclude(q => q.Options)
                .Include(eq => eq.Question!)
                    .ThenInclude(q => q.Module)
                .ToListAsync(ct);

            return list.Select(eq => (eq.Question!, eq)).ToList();
        }
    }
}

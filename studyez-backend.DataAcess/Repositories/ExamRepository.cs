using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.DataAccess.Context;

namespace studyez_backend.DataAcess.Repositories
{
    public sealed class ExamRepository(ApplicationDbContext db) : IExamRepository
    {
        public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct)
            => db.Exams.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<Exam>> GetByCourseAsync(Guid courseId, CancellationToken ct)
            => db.Exams.AsNoTracking().Where(x => x.CourseId == courseId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

        public Task AddAsync(Exam entity, CancellationToken ct)
        {
            db.Exams.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddQuestionAsync(Question q, CancellationToken ct)
        {
            db.Questions.Add(q);
            return Task.CompletedTask;
        }

        public Task AddQuestionOptionAsync(QuestionOption qo, CancellationToken ct)
        {
            db.QuestionOptions.Add(qo);
            return Task.CompletedTask;
        }

        public Task AddExamQuestionAsync(ExamQuestion eq, CancellationToken ct)
        {
            db.ExamQuestions.Add(eq);
            return Task.CompletedTask;
        }

        public Task<bool> HasResultsAsync(Guid examId, CancellationToken ct)
        => db.ExamResults.AnyAsync(r => r.ExamId == examId, ct);

        public async Task<bool> TryDeleteAsync(Guid examId, CancellationToken ct)
        {
            if (await HasResultsAsync(examId, ct)) return false;

            var exam = await db.Exams.FindAsync(new object?[] { examId }, ct);
            if (exam is null) return true;

            db.Exams.Remove(exam);
            return true;
        }

        // Pull the ordered questions for an exam with full question + options graph
        public Task<List<ExamQuestion>> GetExamQuestionsAsync(Guid examId, CancellationToken ct) =>
            db.ExamQuestions
              .AsNoTracking()
              .Where(eq => eq.ExamId == examId)
              .OrderBy(eq => eq.Order)
              .Include(eq => eq.Question)!
                  .ThenInclude(q => q.Options)
              .Include(eq => eq.Question)!
                  .ThenInclude(q => q.Module)  // for module-based scoring
              .ToListAsync(ct);

        public Task AddResultAsync(ExamResult result, CancellationToken ct)
        {
            db.ExamResults.Add(result);
            return Task.CompletedTask;
        }

        public Task AddResultAnswersAsync(IEnumerable<ExamResultAnswer> answers, CancellationToken ct)
        {
            if (answers is not null) db.ExamResultAnswers.AddRange(answers);
            return Task.CompletedTask;
        }

        public Task AddModuleScoresAsync(IEnumerable<ModuleScore> scores, CancellationToken ct)
        {
            if (scores is not null) db.ModuleScores.AddRange(scores);
            return Task.CompletedTask;
        }
    }
}

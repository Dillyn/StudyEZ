using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Course> Courses { get; }
        DbSet<Module> Modules { get; }
        DbSet<Question> Questions { get; }
        DbSet<QuestionOption> QuestionOptions { get; }
        DbSet<Exam> Exams { get; }
        DbSet<ExamQuestion> ExamQuestions { get; }
        DbSet<ExamResult> ExamResults { get; }
        DbSet<ExamResultAnswer> ExamResultAnswers { get; }
        DbSet<ModuleScore> ModuleScores { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

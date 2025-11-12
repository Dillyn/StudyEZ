using Microsoft.EntityFrameworkCore;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces;

namespace studyez_backend.DataAccess.Context
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : DbContext(options), IAppDbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();
        public DbSet<ExamResultAnswer> ExamResultAnswers => Set<ExamResultAnswer>();
        public DbSet<ModuleScore> ModuleScores => Set<ModuleScore>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Set UTC defaults on create/update via SQL default (GETUTCDATE)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entity.ClrType))
                {
                    modelBuilder.Entity(entity.ClrType).Property<DateTime>("CreatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        // auto-touch UpdatedAt
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

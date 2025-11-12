using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using studyez_backend.Core.Entities;
using DbQuestionType = studyez_backend.Core.Constants.Constants.QuestionType;

namespace studyez_backend.DataAccess.Context.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("Users", tb =>
            {
                tb.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Free','Pro','Premium','Admin')");
                tb.HasCheckConstraint("CK_Users_Email", "[Email] LIKE '%@%'");
            });
            b.HasKey(x => x.Id);

            b.Property(x => x.Email).HasMaxLength(255).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();

            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Avatar).HasMaxLength(500);

            b.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(x => x.Role);
            b.HasIndex(x => x.IsActive);
        }
    }

    public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> b)
        {
            b.ToTable("Courses");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Subject).HasMaxLength(100).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.IsDeleted).HasDefaultValue(false);

            b.HasOne(x => x.User)
                .WithMany(u => u.Courses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.IsDeleted);
            b.HasIndex(x => x.CreatedAt).IsDescending();

            // Soft delete filter
            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> b)
        {
            b.ToTable("Modules");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.OriginalContent).IsRequired();
            b.Property(x => x.SimplifiedContent);
            b.Property(x => x.Order).HasDefaultValue(0);
            b.Property(x => x.IsDeleted).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.CourseId);
            b.HasIndex(x => new { x.CourseId, x.Order });
            b.HasIndex(x => x.IsDeleted);

            b.HasQueryFilter(x => !x.IsDeleted);
        }
    }

    public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        // Wrap switch expressions in static methods (allowed inside Expression trees)
        private static string EnumToDb(DbQuestionType v) => v switch
        {
            DbQuestionType.MultipleChoice => "multiple-choice",
            DbQuestionType.TrueFalse => "true-false",
            DbQuestionType.ShortAnswer => "short-answer",
            _ => throw new ArgumentOutOfRangeException(nameof(v), v, null)
        };

        private static DbQuestionType DbToEnum(string v) => v switch
        {
            "multiple-choice" => DbQuestionType.MultipleChoice,
            "true-false" => DbQuestionType.TrueFalse,
            "short-answer" => DbQuestionType.ShortAnswer,
            _ => throw new ArgumentOutOfRangeException(nameof(v), v, null)
        };

        public void Configure(EntityTypeBuilder<Question> b)
        {
            // Use the static methods inside the converter
            var enumToString = new ValueConverter<DbQuestionType, string>(
                v => EnumToDb(v),
                v => DbToEnum(v));

            b.ToTable("Questions", tb =>
            {
                tb.HasCheckConstraint(
                    "CK_Questions_Type",
                    "[Type] IN ('multiple-choice','true-false','short-answer')");
            });

            b.HasKey(x => x.Id);

            b.Property(x => x.Type)               // <-- property must be your enum type
                .HasConversion(enumToString)      // store hyphenated string in SQL
                .HasMaxLength(50)
                .IsRequired();

            b.Property(x => x.QuestionText).HasMaxLength(1000).IsRequired();
            b.Property(x => x.CorrectAnswer).HasMaxLength(1000).IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.Module)
                .WithMany(m => m.Questions)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.ModuleId);
            b.HasIndex(x => x.Type);

            b.HasQueryFilter(q => q.Module != null && !q.Module.IsDeleted);
        }
    }

    public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
    {
        public void Configure(EntityTypeBuilder<QuestionOption> b)
        {
            b.ToTable("QuestionOptions");
            b.HasKey(x => x.Id);

            b.Property(x => x.OptionText).HasMaxLength(500).IsRequired();
            b.Property(x => x.Order).HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasIndex(x => x.QuestionId);
            b.HasIndex(x => new { x.QuestionId, x.Order });

            // Matching filter: Question has Module filter (!IsDeleted)
            b.HasQueryFilter(qo =>
                qo.Question != null &&
                qo.Question.Module != null &&
                !qo.Question.Module.IsDeleted);
        }
    }

    public sealed class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> b)
        {
            b.ToTable("Exams");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).HasMaxLength(200);
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.Course)
                .WithMany(c => c.Exams)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CourseId);
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.CreatedAt).IsDescending();

            b.HasQueryFilter(e => e.Course != null && !e.Course.IsDeleted);
        }
    }

    public sealed class ExamQuestionConfiguration : IEntityTypeConfiguration<ExamQuestion>
    {
        public void Configure(EntityTypeBuilder<ExamQuestion> b)
        {
            b.ToTable("ExamQuestions", tb =>
            {
                tb.HasCheckConstraint("CK_ExamQuestions_Points", "[Points] > 0");
            });
            b.HasKey(x => x.Id);

            b.Property(x => x.Points).HasColumnType("decimal(5,2)").HasDefaultValue(1.0m);
            b.Property(x => x.Order).HasDefaultValue(0);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.Exam)
                .WithMany(e => e.ExamQuestions)
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne(x => x.Question)
                .WithMany(q => q.ExamQuestions)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasIndex(x => x.ExamId);
            b.HasIndex(x => x.QuestionId);
            b.HasIndex(x => new { x.ExamId, x.QuestionId }).IsUnique();

            // Matching filter: Exam has filter (e => e.Course != null && !e.Course.IsDeleted)
            b.HasQueryFilter(eq =>
                eq.Exam != null &&
                eq.Exam.Course != null &&
                !eq.Exam.Course.IsDeleted);
        }
    }

    public sealed class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
    {
        public void Configure(EntityTypeBuilder<ExamResult> b)
        {
            b.ToTable("ExamResults", tb =>
            {
                tb.HasCheckConstraint(
                    "CK_ExamResults_OverallScore",
                    "[OverallScore] >= 0 AND [OverallScore] <= 100");
                tb.HasCheckConstraint(
                    "CK_ExamResults_Answers",
                    "[CorrectAnswers] >= 0 AND [CorrectAnswers] <= [TotalQuestions]");
            });
            b.HasKey(x => x.Id);

            b.Property(x => x.OverallScore).HasColumnType("decimal(5,2)").IsRequired();
            b.Property(x => x.CompletedAt).HasDefaultValueSql("GETUTCDATE()");
            b.Property(x => x.TimeTaken);

            b.HasOne(x => x.Exam)
                .WithMany(e => e.ExamResults)
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne(x => x.User)
                .WithMany(u => u.ExamResults)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasIndex(x => x.ExamId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.CompletedAt).IsDescending();
            b.HasIndex(x => new { x.UserId, x.ExamId });

            // Matching filter: Exam has filter (e => e.Course != null && !e.Course.IsDeleted)
            b.HasQueryFilter(er =>
                er.Exam != null &&
                er.Exam.Course != null &&
                !er.Exam.Course.IsDeleted);
        }
    }

    public sealed class ExamResultAnswerConfiguration : IEntityTypeConfiguration<ExamResultAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamResultAnswer> b)
        {
            b.ToTable("ExamResultAnswers");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserAnswer).HasMaxLength(1000).IsRequired();
            b.Property(x => x.IsCorrect).IsRequired();
            b.Property(x => x.Points).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.ExamResult)
                .WithMany(r => r.Answers)
                .HasForeignKey(x => x.ExamResultId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasIndex(x => x.ExamResultId);
            b.HasIndex(x => x.QuestionId);

            // Matching filters:
            // - Question -> Module (!IsDeleted)
            // - ExamResult -> Exam -> Course (!IsDeleted)
            b.HasQueryFilter(a =>
                a.Question != null &&
                a.Question.Module != null &&
                !a.Question.Module.IsDeleted &&
                a.ExamResult != null &&
                a.ExamResult.Exam != null &&
                a.ExamResult.Exam.Course != null &&
                !a.ExamResult.Exam.Course.IsDeleted);
        }
    }

    public sealed class ModuleScoreConfiguration : IEntityTypeConfiguration<ModuleScore>
    {
        public void Configure(EntityTypeBuilder<ModuleScore> b)
        {
            b.ToTable("ModuleScores", tb =>
            {
                tb.HasCheckConstraint(
                    "CK_ModuleScores_Score",
                    "[Score] >= 0 AND [Score] <= 100");
                tb.HasCheckConstraint(
                    "CK_ModuleScores_Answers",
                    "[CorrectCount] >= 0 AND [CorrectCount] <= [QuestionsCount]");
            });
            b.HasKey(x => x.Id);

            b.Property(x => x.Score).HasColumnType("decimal(5,2)").IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(x => x.ExamResult)
                .WithMany(r => r.ModuleScores)
                .HasForeignKey(x => x.ExamResultId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Module)
                .WithMany()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.ExamResultId);
            b.HasIndex(x => x.ModuleId);
            b.HasIndex(x => new { x.ExamResultId, x.ModuleId }).IsUnique();

            b.HasQueryFilter(ms => ms.Module != null && !ms.Module.IsDeleted);
        }
    }
}

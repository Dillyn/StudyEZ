using studyez_backend.Core.Constants;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Exceptions;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Utils;

namespace studyez_backend.Services.Services
{
    public sealed class ExamRunService(
    IExamRepository exams,
    IUnitOfWork uow) : IExamRunService
    {
        private readonly IExamRepository _exams = exams;
        private readonly IUnitOfWork _uow = uow;

        public async Task<ExamStartDto> StartAsync(Guid examId, CancellationToken ct)
        {
            var exam = await _exams.GetByIdAsync(examId, ct) ?? throw new ExamNotFoundException(examId);
            var links = await _exams.GetExamQuestionsAsync(examId, ct);
            if (links.Count == 0) throw new ValidationException("Exam has no questions.");

            var items = links.Select(l =>
            {
                var q = l.Question!;
                var opts = q.Type == Constants.QuestionType.MultipleChoice
                    ? q.Options.OrderBy(o => o.Order).Select(o => o.OptionText).ToList()
                    : null;

                return new ExamQuestionItemDto(q.Id, q.TypeString(), q.QuestionText, opts, l.Order, l.Points);
            })
            .OrderBy(i => i.Order)
            .ToList();

            return new ExamStartDto(examId, exam.Title, items.Count, items);
        }

        public async Task<ExamResult> SubmitAsync(SubmitExamRequest request, Guid actorUserId, CancellationToken ct)
        {
            var links = await _exams.GetExamQuestionsAsync(request.ExamId, ct);
            if (links.Count == 0) throw new ValidationException("Exam has no questions.");

            var ansByQid = request.Answers.ToDictionary(a => a.QuestionId, a => a.UserAnswer?.Trim() ?? "");

            int total = links.Count;
            int correct = 0;
            var answers = new List<ExamResultAnswer>(total);

            foreach (var link in links)
            {
                var q = link.Question!;
                var userText = ansByQid.TryGetValue(q.Id, out var ua) ? ua : "";
                bool isCorrect = IsCorrect(q, userText);
                if (isCorrect) correct++;

                answers.Add(new ExamResultAnswer
                {
                    Id = Guid.NewGuid(),
                    ExamResultId = Guid.Empty,
                    QuestionId = q.Id,
                    UserAnswer = userText,
                    IsCorrect = isCorrect,
                    Points = isCorrect ? link.Points : 0m,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var overallPercent = total == 0 ? 0m : Math.Round((decimal)correct / total * 100m, 2);

            var result = new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = request.ExamId,
                UserId = actorUserId,
                OverallScore = overallPercent,
                TotalQuestions = total,
                CorrectAnswers = correct,
                CompletedAt = DateTime.UtcNow
            };
            await _exams.AddResultAsync(result, ct);

            foreach (var a in answers) a.ExamResultId = result.Id;
            await _exams.AddResultAnswersAsync(answers, ct);

            // Module scores
            var byModule = links
                .GroupBy(l => l.Question!.ModuleId)
                .Select(g =>
                {
                    var qids = g.Select(x => x.QuestionId).ToHashSet();
                    var corr = answers.Count(a => a.IsCorrect && qids.Contains(a.QuestionId));
                    var cnt = qids.Count;
                    var pct = cnt == 0 ? 0m : Math.Round((decimal)corr / cnt * 100m, 2);

                    return new ModuleScore
                    {
                        Id = Guid.NewGuid(),
                        ExamResultId = result.Id,
                        ModuleId = g.Key,
                        Score = pct,
                        QuestionsCount = cnt,
                        CorrectCount = corr,
                        CreatedAt = DateTime.UtcNow
                    };
                })
                .ToList();

            await _exams.AddModuleScoresAsync(byModule, ct);

            await _uow.SaveChangesAsync(ct);
            return result;
        }

        private static bool IsCorrect(Question q, string userAnswer)
        {
            static string N(string s) => (s ?? "").Trim().ToLowerInvariant();

            if (q.Type == Constants.QuestionType.TrueFalse)
            {
                var norm = N(userAnswer);
                if (norm is "t" or "true" or "1" or "yes" or "y") norm = "true";
                if (norm is "f" or "false" or "0" or "no" or "n") norm = "false";
                return N(q.CorrectAnswer) == norm;
            }

            return N(q.CorrectAnswer) == N(userAnswer);
        }
    }
}

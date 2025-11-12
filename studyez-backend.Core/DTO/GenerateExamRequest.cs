using System.ComponentModel.DataAnnotations;

namespace studyez_backend.Core.DTO
{
    public sealed class GenerateExamRequest
    {
        [Required] public Guid CourseId { get; init; }
        [Range(1, 200)] public int TotalQuestions { get; init; } = 20;

        // Optional knobs for later
        [Range(0, 100)] public int McqPercent { get; init; } = 70;
        [Range(0, 100)] public int TrueFalsePercent { get; init; } = 20;
        [Range(0, 100)] public int ShortAnswerPercent { get; init; } = 10;
        public bool StrictJson { get; init; } = true;
    }
}

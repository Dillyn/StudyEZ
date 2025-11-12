using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>Exam run lifecycle: start and submit.</summary>
    [ApiController]
    [Route("api/exam-runs")]
    public sealed class ExamRunsController(
    IExamRunService runs,
    ICurrentUser currentUser) : ControllerBase
    {
        private readonly IExamRunService _runs = runs;
        private readonly ICurrentUser _current = currentUser;

        /// <summary>Start an exam: returns ordered items and metadata.</summary>
        [Authorize]
        [HttpGet("start/{examId:guid}")]
        [ProducesResponseType(typeof(ExamStartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExamStartDto>> Start(Guid examId, CancellationToken ct)
            => Ok(await _runs.StartAsync(examId, ct));

        /// <summary>Submit answers for grading and persistence.</summary>
        [Authorize]
        [HttpPost("submit")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> Submit([FromBody] SubmitExamRequest req, CancellationToken ct)
        {
            // Service returns the saved ExamResult entity.
            ExamResult saved = await _runs.SubmitAsync(req, _current.UserId, ct);

            // Respond with a concise summary payload 
            var summary = new
            {
                saved.Id,
                saved.ExamId,
                saved.UserId,
                saved.OverallScore,
                saved.TotalQuestions,
                saved.CorrectAnswers,
                saved.CompletedAt
            };
            return Ok(summary);
        }
    }
}

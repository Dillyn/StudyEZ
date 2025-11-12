using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>Query exam results.</summary>
    [ApiController]
    [Route("api/exam-results")]
    public sealed class ExamResultsController(
    IExamResultService results,
    ICurrentUser currentUser) : ControllerBase
    {
        private readonly IExamResultService _results = results;
        private readonly ICurrentUser _current = currentUser;

        /// <summary>Get a result by id.</summary>
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ExamResultSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExamResultSummaryDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _results.GetAsync(id, _current.UserId, _current.Role, ct));

        /// <summary>List results for a user. Admin sees any user; non-admin only themselves.</summary>
        [Authorize]
        [HttpGet("by-user/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ExamResultSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IReadOnlyList<ExamResultSummaryDto>>> GetByUser(Guid userId, CancellationToken ct)
            => Ok(await _results.GetByUserAsync(userId, _current.UserId, _current.Role, ct));

        /// <summary>List results for an exam. Admin gets all; non-admin only their own.</summary>
        [Authorize]
        [HttpGet("by-exam/{examId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ExamResultSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IReadOnlyList<ExamResultSummaryDto>>> GetByExam(Guid examId, CancellationToken ct)
            => Ok(await _results.GetByExamAsync(examId, _current.UserId, _current.Role, ct));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>Manage exams (read, generate, delete).</summary>
    [ApiController]
    [Route("api/exams")]
    public sealed class ExamsController(
    IExamService exams,
    ICurrentUser currentUser) : ControllerBase
    {
        private readonly IExamService _exams = exams;
        private readonly ICurrentUser _current = currentUser;

        /// <summary>Get an exam by id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ExamDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExamDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _exams.GetAsync(id, ct));

        /// <summary>List exams for a course.</summary>
        [HttpGet("by-course/{courseId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<ExamDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ExamDto>>> GetByCourse(Guid courseId, CancellationToken ct)
            => Ok(await _exams.GetByCourseAsync(courseId, ct));

        /// <summary>Generate a course-wide exam using AI.</summary>
        [Authorize]
        [HttpPost("generate")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ExamDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Generate([FromBody] ExamCreateCommand cmd, CancellationToken ct)
        {
            var dto = await _exams.GenerateAsync(cmd, _current.UserId, _current.Role, ct);
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        /// <summary>Delete an exam (forbidden if it already has results).</summary>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _exams.DeleteAsync(id, _current.UserId, _current.Role, ct);
            return NoContent();
        }
    }
}

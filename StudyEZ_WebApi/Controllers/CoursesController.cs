using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>
    /// Course endpoints: CRUD with soft-delete, ownership checks, and auditing via service layer.
    /// </summary>
    [ApiController]
    [Route("api/courses")]
    public sealed class CoursesController(
        ICourseService courses,
        ICurrentUser currentUser,
        ILogger<CoursesController> log) : ControllerBase
    {
        private readonly ICourseService _courses = courses;
        private readonly ICurrentUser _current = currentUser;
        private readonly ILogger<CoursesController> _log = log;

        /// <summary>
        /// Get a course by its identifier.
        /// </summary>
        /// <param name="id">Course id.</param>
        /// <response code="200">Returns the course.</response>
        /// <response code="404">Course not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken ct)
            => Ok(await _courses.GetAsync(id, ct));

        /// <summary>
        /// List courses for a specific user.
        /// </summary>
        /// <remarks>
        /// Admins can query any user. Non-admins can only query their own user id.
        /// </remarks>
        /// <param name="userId">Owner user id.</param>
        [Authorize]
        [HttpGet("by-user/{userId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<FetchCourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IReadOnlyList<FetchCourseDto>>> GetByUser(Guid userId, CancellationToken ct)
            => Ok(await _courses.GetByUserAsync(userId, _current.UserId, _current.Role, ct));

        /// <summary>
        /// Create a new course.
        /// </summary>
        /// <remarks>
        /// Writes audit columns (CreatedBy/CreatedAt) in the service layer.
        /// </remarks>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateCourseRequest req, CancellationToken ct)
        {
            var dto = await _courses.CreateAsync(
                new CreateCourseCommand(req.UserId, req.Name, req.Subject, req.Description),
                _current.UserId, _current.Role, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Update an existing course.
        /// </summary>
        /// <remarks>
        /// Enforces ownership/Admin; writes UpdatedBy/UpdatedAt.
        /// </remarks>
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest req, CancellationToken ct)
        {
            var dto = await _courses.UpdateAsync(
                id,
                new UpdateCourseCommand(req.Name, req.Subject, req.Description),
                _current.UserId, _current.Role, ct);
            return Ok(dto);
        }

        /// <summary>
        /// Soft delete a course (set IsDeleted = true).
        /// </summary>
        /// <remarks>
        /// Use <c>?hardDelete=true</c> to physically remove. Ownership/Admin enforced.
        /// </remarks>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hardDelete, CancellationToken ct)
        {
            await _courses.DeleteAsync(id, _current.UserId, _current.Role, ct, hardDelete);
            return NoContent();
        }

        /// <summary>
        /// Restore a previously soft-deleted course.
        /// </summary>
        /// <remarks>Bypasses global query filters in the repository.</remarks>
        [Authorize]
        [HttpPost("{id:guid}/restore")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
            => Ok(await _courses.RestoreAsync(id, _current.UserId, _current.Role, ct));
    }

}
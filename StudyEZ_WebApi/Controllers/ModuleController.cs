using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>
    /// Module endpoints: CRUD, upload, AI simplification, soft-delete/restore.
    /// </summary>
    [ApiController]
    [Route("api/modules")]
    public sealed class ModulesController(
        IModuleService modules,
        ICurrentUser currentUser,
        ILogger<ModulesController> log) : ControllerBase
    {
        private readonly IModuleService _modules = modules;
        private readonly ICurrentUser _current = currentUser;
        private readonly ILogger<ModulesController> _log = log;

        /// <summary>
        /// Get a module by id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ModuleDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _modules.GetAsync(id, ct));

        /// <summary>
        /// List modules for a course (excludes soft-deleted by default).
        /// </summary>
        [HttpGet("by-course/{courseId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<FetchModuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<FetchModuleDto>>> GetByCourse(Guid courseId, CancellationToken ct)
            => Ok(await _modules.GetByCourseAsync(courseId, ct));

        /// <summary>
        /// Create a new module (inline text content).
        /// </summary>
        /// <remarks>Writes audit columns; enforces ownership/Admin.</remarks>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(FetchModuleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateModuleRequest req, CancellationToken ct)
        {
            var dto = await _modules.CreateAsync(
                new CreateModuleCommand(req.CourseId, req.Title, req.Order, req.OriginalContent),
                _current.UserId, _current.Role, ct);

            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        // TODO: Enable file upload once file storage is set up.
        /// <summary>
        /// Upload a module file (TXT/MD/PDF extraction handled on your side if needed).
        /// </summary>
        /// <remarks>Consumes <c>multipart/form-data</c>.</remarks>
        //[Authorize]
        //[HttpPost("upload")]
        //[Consumes("multipart/form-data")]
        //[ProducesResponseType(typeof(ModuleDto), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //public async Task<IActionResult> Upload([FromForm] UploadModuleFileRequest req, CancellationToken ct)
        //{
        //    var dto = await _modules.UploadAsync(
        //        new UploadModuleCommand(req.CourseId, req.Title, req.Order, req.File.OpenReadStream()),
        //        _current.UserId, _current.Role, ct);

        //    return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        //}

        /// <summary>
        /// Simplify a module's original content using Azure AI.
        /// </summary>
        /// <remarks>Stores the simplified Markdown in the module.</remarks>
        [Authorize]
        [HttpPost("{id:guid}/simplify")]
        [ProducesResponseType(typeof(FetchModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Simplify(Guid id, CancellationToken ct)
            => Ok(await _modules.SimplifyAsync(id, _current.UserId, _current.Role, ct));

        /// <summary>
        /// Update a module.
        /// </summary>
        /// <remarks>Ownership/Admin enforced. Writes UpdatedBy/UpdatedAt.</remarks>
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(FetchModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateModuleRequest req, CancellationToken ct)
        {
            var dto = await _modules.UpdateAsync(
                id,
                new UpdateModuleCommand(req.Title, req.Order, req.OriginalContent),
                _current.UserId, _current.Role, ct);

            return Ok(dto);
        }

        /// <summary>
        /// Update only the simplified content of a module.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("{id:guid}/simplified")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateSimplified(Guid id, [FromBody] UpdateModuleSimplifiedRequest req, CancellationToken ct)
        {
            var dto = await _modules.UpdateSimplifiedAsync(
                id,
                req.SimplifiedContent,
                _current.UserId,
                _current.Role,
                ct);

            return Ok(dto);
        }

        /// <summary>
        /// Soft delete (or hard delete with <c>?hard=true</c>) a module.
        /// </summary>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard, CancellationToken ct)
        {
            await _modules.DeleteAsync(id, _current.UserId, _current.Role, ct, hard);
            return NoContent();
        }

        /// <summary>
        /// Restore a soft-deleted module.
        /// </summary>
        [Authorize]
        [HttpPost("{id:guid}/restore")]
        [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
            => Ok(await _modules.RestoreAsync(id, _current.UserId, _current.Role, ct));
    }
}

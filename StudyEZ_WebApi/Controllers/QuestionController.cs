using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>Manage standalone questions (CRUD).</summary>
    [ApiController]
    [Route("api/questions")]
    public sealed class QuestionsController(
     IQuestionService questions,
     ICurrentUser currentUser) : ControllerBase
    {
        private readonly IQuestionService _questions = questions;
        private readonly ICurrentUser _current = currentUser;

        /// <summary>Get a question by id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(QuestionReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestionReadDto>> Get(Guid id, CancellationToken ct)
        {
            var dto = await _questions.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        /// <summary>List questions for a module.</summary>
        [HttpGet("by-module/{moduleId:guid}")]
        [ProducesResponseType(typeof(IReadOnlyList<QuestionReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<QuestionReadDto>>> GetByModule(Guid moduleId, CancellationToken ct)
        {
            var list = await _questions.GetByModuleAsync(moduleId, ct);
            return Ok(list);
        }

        /// <summary>Create a new question.</summary>
        [Authorize]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] QuestionCreateDto dto, CancellationToken ct)
        {
            var id = await _questions.CreateAsync(dto, _current.UserId, _current.Role, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }

        /// <summary>Update an existing question.</summary>
        [Authorize]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] QuestionUpdateDto dto, CancellationToken ct)
        {
            await _questions.UpdateAsync(id, dto, _current.UserId, _current.Role, ct);
            return NoContent();
        }

        /// <summary>Delete a question.</summary>
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _questions.DeleteAsync(id, _current.UserId, _current.Role, ct);
            return NoContent();
        }
    }
}

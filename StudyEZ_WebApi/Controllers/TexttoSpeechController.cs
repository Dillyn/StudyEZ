using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.Security.Claims;
using studyez_backend.Services.Ai.AiSpeech;

namespace StudyEZ_WebApi.Controllers
{
    [ApiController]
    [Route("api/tts")]
    public sealed class TextToSpeechController(ISpeechClient speech, ICurrentUser current)
    : ControllerBase
    {
        private readonly ISpeechClient _speech = speech;
        private readonly ICurrentUser _current = current;

        public sealed class TtsRequest
        {
            public string Text { get; init; } = default!;
            public string? Voice { get; init; }
        }

        [Authorize]
        [HttpPost]
        [Produces("audio/mpeg")]
        public async Task<IActionResult> Speak([FromBody] TtsRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Text))
                return BadRequest("Text is required.");

            var bytes = await _speech.SynthesizeAsync(req.Text, req.Voice, ct);
            // mp3 – matches OutputFormat
            return File(bytes, "audio/mpeg");
        }


        // GET api/dev/tts?text=hello
        [HttpGet]
        public async Task<IActionResult> Test([FromQuery] string text = "Hello from StudyEZ")
        {
            var bytes = await _speech.SynthesizeAsync(text, voice: null, HttpContext.RequestAborted);

            return File(bytes, "audio/mpeg", "test-tts.mp3");
        }

    }
}

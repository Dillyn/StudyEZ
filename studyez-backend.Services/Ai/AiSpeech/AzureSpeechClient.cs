using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using studyez_backend.Core.Exceptions;
using studyez_backend.Services.Helper;

namespace studyez_backend.Services.Ai.AiSpeech
{
    public sealed class AzureSpeechClient : ISpeechClient
    {
        private readonly HttpClient _http;
        private readonly AzureSpeechOptions _opt;
        private readonly ILogger<AzureSpeechClient> _log;

        public AzureSpeechClient(
            HttpClient http,
            IOptions<AzureSpeechOptions> opt,
            ILogger<AzureSpeechClient> log)
        {
            _http = http;
            _opt = opt.Value;
            _log = log;

        }

        public async Task<byte[]> SynthesizeAsync(string text, string? voice, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new TtsBadRequestException("Text must not be empty.");

            var v = string.IsNullOrWhiteSpace(voice) ? _opt.DefaultVoice : voice.Trim();
            if (string.IsNullOrWhiteSpace(v))
                throw new TtsConfigurationException("No voice configured for Azure Speech.");

            if (string.IsNullOrWhiteSpace(_opt.ApiKey))
                throw new TtsConfigurationException("Azure Speech API key is not configured.");

            // Build request
            var req = new HttpRequestMessage(HttpMethod.Post, "cognitiveservices/v1");

            req.Headers.Add("Ocp-Apim-Subscription-Key", _opt.ApiKey);
            req.Headers.Add("X-Microsoft-OutputFormat", _opt.OutputFormat);
            req.Headers.Add("User-Agent", "StudyEz-TTS");

            var plain = Helpers.StripMarkdown(text);
            var escaped = System.Security.SecurityElement.Escape(plain);
            var ssml = $@"
<speak version='1.0' xml:lang='en-US'>
  <voice xml:lang='en-US' name='{v}'>{escaped}</voice>
</speak>";

            req.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

            _log.LogInformation("Calling TTS. BaseAddress={BaseAddress}, Path={Path}",
                _http.BaseAddress, "cognitiveservices/v1");

            using var resp = await _http.SendAsync(req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _log.LogError("TTS failed with {StatusCode}: {Body}", resp.StatusCode, body);
                throw new HttpRequestException($"TTS failed {resp.StatusCode}: {body}");
            }

            return await resp.Content.ReadAsByteArrayAsync(ct);
        }
    }
}

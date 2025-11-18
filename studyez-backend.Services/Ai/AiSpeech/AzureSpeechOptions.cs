namespace studyez_backend.Services.Ai.AiSpeech
{
    public sealed class AzureSpeechOptions
    {
        public string Endpoint { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string DefaultVoice { get; set; } = "en-US-AriaNeural";
        public string OutputFormat { get; set; } = "audio-24khz-48kbitrate-mono-mp3";
    }
}

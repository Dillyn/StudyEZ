namespace studyez_backend.Services.Ai.AiSpeech
{
    public interface ISpeechClient
    {
        Task<byte[]> SynthesizeAsync(string text, string? voice, CancellationToken ct = default);
    }
}

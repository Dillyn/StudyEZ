namespace studyez_backend.Core.Interfaces
{
    public interface IAiClient
    {
        /// <summary>
        /// Simplifies the given content using AI services.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string> SimplifyAsync(string content, CancellationToken ct = default);
    }
}


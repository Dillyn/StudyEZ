namespace studyez_backend.Core.Interfaces
{
    public interface IaoiRestClient
    {
        /// <summary>
        /// Sends a chat request to the AI service and returns the reply along with token usage statistics.
        /// </summary>
        /// <param name="deployment"></param>
        /// <param name="system"></param>
        /// <param name="user"></param>
        /// <param name="temperature"></param>
        /// <param name="maxTokens"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(string reply, int? promptTokens, int? completionTokens, int? totalTokens)>
            ChatAsync(string deployment, string system, string user, float temperature = 0.2f, int maxTokens = 256, CancellationToken ct = default);
    }
}

namespace studyez_backend.Core.Interfaces
{
    public interface IAoaiRestClient
    {
        /// <summary>
        /// Sends a chat/completions request. 
        /// If UseGlobalEndpoint = true, idOrDeployment is a model name. Otherwise a deployment name.
        /// </summary>
        Task<string> ChatAsync(string idOrDeployment, object body, CancellationToken ct = default);
    }
}

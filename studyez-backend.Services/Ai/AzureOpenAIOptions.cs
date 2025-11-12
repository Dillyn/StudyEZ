namespace studyez_backend.Services.Ai
{
    public sealed class AzureOpenAIOptions
    {
        public string Endpoint { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public bool UseGlobalEndpoint { get; set; } = true;
        public string ApiVersion { get; set; } = "2025-01-01-preview";
        public string SimplifyModelOrDeployment { get; set; } = "gpt-5-mini";
        public string ExamModelOrDeployment { get; set; } = "gpt-4.1";
    }
}

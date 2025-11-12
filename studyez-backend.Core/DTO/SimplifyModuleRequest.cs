using System.ComponentModel.DataAnnotations;

namespace studyez_backend.Core.DTO
{
    public sealed class SimplifyModuleRequest
    {
        [Required] public Guid ModuleId { get; init; }
    }
}

using System.Security.Claims;

namespace studyez_backend.Core.Security.Claims
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid UserId { get; }
        string Role { get; }
        ClaimsPrincipal Principal { get; }
    }
}

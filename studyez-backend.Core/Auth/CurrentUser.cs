using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using studyez_backend.Core.Security.Claims;


namespace studyez_backend.Core.Auth
{


    public class CurrentUser(IHttpContextAccessor accessor, IHostEnvironment env) : ICurrentUser
    {
        private ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();

        public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;
        public Guid UserId
        {
            get
            {

                var id =
                    User.FindFirst(ClaimTypesEx.UserId)?.Value ??
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    User.FindFirst("sub")?.Value;

                // DEV OVERRIDE: ?asUser=<guid>
                if (env.IsDevelopment())
                {
                    var q = accessor.HttpContext?.Request.Query["asUser"].ToString();
                    if (!string.IsNullOrWhiteSpace(q)) id = q!;
                }

                return Guid.TryParse(id, out var g) ? g : Guid.Empty;
            }
        }

        public string Role
        {
            get
            {
                // Prefer app-specific role -> then standard role
                var r =
                    User.FindFirst(ClaimTypesEx.Role)?.Value ??
                    User.FindFirst(ClaimTypes.Role)?.Value ??
                    "Free";

                // DEV OVERRIDE: ?asRole=Admin/Pro/Premium/Free
                if (env.IsDevelopment())
                {
                    var q = accessor.HttpContext?.Request.Query["asRole"].ToString();
                    if (!string.IsNullOrWhiteSpace(q)) r = q!;
                }

                return r;
            }
        }

        public ClaimsPrincipal Principal => User;

        private static string? FindFirstValue(ClaimsPrincipal user, string claimType)
        {
            return user?.FindFirst(claimType)?.Value;
        }
    }
}

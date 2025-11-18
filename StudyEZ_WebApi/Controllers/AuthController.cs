using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.DTO;

namespace StudyEZ_WebApi.Controllers
{
    /// <summary>
    /// Google OAuth endpoints: login, logout, whoami, and an auth-restricted demo route.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        // TODO look into OAuth flow returning cors errors on frontend - try different flow   
        /// <summary>
        /// Start Google OAuth login. Redirects to Google consent; returns to <c>RedirectUri</c>.
        /// </summary>
        /// <param name="redirectUri">Optional target after login (defaults to "/").</param>
        [HttpGet("logintest")]
        [AllowAnonymous]
        public IActionResult LoginTest([FromQuery] string? redirectUri = "/")
        {
            var props = new AuthenticationProperties { RedirectUri = redirectUri ?? "/" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }



        /// <summary>
        /// Start Google OAuth login.</c>.
        /// </summary>
        /// <param name="redirectUri">Optional target after login (defaults to "/").</param>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] GoogleCredentialRequest body)
        {

            var payload = await GoogleJsonWebSignature.ValidateAsync(body.credential);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, payload.Subject),
                new Claim(ClaimTypes.Name, payload.Name ?? ""),
                new Claim(ClaimTypes.Email, payload.Email),
                new Claim("picture", payload.Picture ?? "")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(14) });



            return Ok(new { email = payload.Email, name = payload.Name });
        }

        /// <summary>
        /// Logout (clears the cookie) and redirect.
        /// </summary>
        /// <param name="redirectUri">Optional target after logout (defaults to "/").</param>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] string? redirectUri = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(redirectUri ?? "/");
        }

        /// <summary>
        /// Show current authentication/claims information.
        /// </summary>
        [HttpGet("me")]
        [AllowAnonymous]
        public IActionResult Me()
        {
            var u = HttpContext.User;
            var claims = u.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new
            {
                IsAuthenticated = u.Identity?.IsAuthenticated ?? false,
                Name = u.Identity?.Name,
                Claims = claims
            });
        }

        /// <summary>
        /// A route that requires authentication.
        /// </summary>
        [HttpGet("require")]
        [Authorize]
        public IActionResult Require() => Ok(new { message = "Authorized!" });
    }
}

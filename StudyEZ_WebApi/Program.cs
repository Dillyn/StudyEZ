using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using studyez_backend.Core.Auth;
using studyez_backend.Core.Constants;
using studyez_backend.Core.Interfaces;
using studyez_backend.Core.Interfaces.Repositories;
using studyez_backend.Core.Interfaces.Services;
using studyez_backend.Core.Security.Claims;
using studyez_backend.DataAccess.Context;
using studyez_backend.DataAcess.Repositories;
using studyez_backend.Services.Ai;
using studyez_backend.Services.Services;
using studyez_backend.Services.Services.ExamResults;
using StudyEZ_WebApi.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

const string FrontendCors = "Frontend.CORS";

// CORS: allow React origin and credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

    })
    .AddCookie(o =>
    {
        o.LoginPath = "/api/auth/login";
        o.LogoutPath = "/api/auth/logout";
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromDays(14);

        o.Cookie.SameSite = SameSiteMode.None;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;

        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                string? email = ctx.Identity?.FindFirst(ClaimTypes.Email)?.Value
                                    ?? ctx.User.TryGetString("email");

                string? name = ctx.Identity?.FindFirst(ClaimTypes.Name)?.Value
                                 ?? ctx.User.TryGetString("name")
                                 ?? email;

                string? avatar = ctx.User.TryGetString("picture");


                var scope = ctx.HttpContext.RequestServices;
                var users = scope.GetRequiredService<IUserService>();
                var dto = await users.UpsertFromGoogleAsync(email!, name!, avatar, ctx.HttpContext.RequestAborted);


                var id = ctx.Identity!;
                id.AddClaim(new Claim(ClaimTypesEx.UserId, dto.Id.ToString()));
                id.AddClaim(new Claim(ClaimTypesEx.Email, dto.Email));
                id.AddClaim(new Claim(ClaimTypesEx.Name, dto.Name));
                id.AddClaim(new Claim(ClaimTypesEx.Role, dto.Role));
                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                    id.AddClaim(new Claim(ClaimTypesEx.Avatar, dto.Avatar!));


                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, dto.Id.ToString()));
                id.AddClaim(new Claim(ClaimTypes.Email, dto.Email));
                id.AddClaim(new Claim(ClaimTypes.Name, dto.Name));
                id.AddClaim(new Claim(ClaimTypes.Role, dto.Role));
            }
        };
    });

// Athorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Azure OpenAI options
builder.Services.Configure<AzureOpenAIOptions>(builder.Configuration.GetSection("AzureOpenAI"));

// Typed HttpClient for IAoaiRestClient (named "aoai")
builder.Services.AddHttpClient<IAoaiRestClient, AoaiRestClient>("aoai", (sp, http) =>
{
    var o = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    http.BaseAddress = new Uri(o.Endpoint.TrimEnd('/') + "/");
    http.DefaultRequestHeaders.Accept.Clear();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (http.DefaultRequestHeaders.Contains("api-key")) http.DefaultRequestHeaders.Remove("api-key");
    http.DefaultRequestHeaders.Add("api-key", o.ApiKey);
});


// AI adapters
builder.Services.AddScoped<IAiClient, AzureAiClient>();          // simplifier
builder.Services.AddScoped<IExamGenerator, AzureExamGenerator>(); // exam generator

// Repositories & UoW
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamRunService, ExamRunService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

// TODO remove later
// Add a test user in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    if (!db.Users.Any(u => u.Id == id))
    {
        db.Users.Add(new studyez_backend.Core.Entities.User
        {
            Id = id,
            Email = "test.user@studyez.local",
            Name = "Test User",
            PasswordHash = "dev-only",
            Role = Constants.UserRole.Free,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(FrontendCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();
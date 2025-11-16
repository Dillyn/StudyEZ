# StudyEZ — Backend (.NET 8)

An API for **StudyEZ**, an AI-assisted study app. It lets learners create courses, upload/author modules, auto-simplify content with AI, generate exams, take them, and view results with Google OAuth sign-in and cookie auth.

> Status: **Active development**
> 
> - Frontend (React + Mantine) is in progress.
>     
> - **Unit tests** and broader integration tests are planned but not yet committed.
>     

---

## Features

- **Auth**
    
    - Google OAuth → cookie auth.
        
    - First login **upserts** a `User` record (email/name/avatar/role) and sets claims.
        
- **Users & Roles**
    
    - Roles: `Free | Pro | Premium | Admin`.
        
    - `ICurrentUser` abstraction resolves `UserId`/`Role` from claims; dev overrides via `?asUser=` / `?asRole=` in **Development**.
        
- **Courses & Modules**
    
    - Full CRUD with ownership guards (owner or admin).
        
    - Module content can be **AI-simplified** (Azure OpenAI).
        
- **Exams**
    
    - Generate from course modules (configurable total questions).
        
    - Start exam (ordered items, MCQ/TF/Short).
        
    - Submit answers → auto-grade → per-module scores + overall %.
        
- **Error handling**
    
    - Central **ExceptionMappingMiddleware** maps domain exceptions → HTTP (400/403/404/409/500).
        
- **Architecture**
    
    - Onion Architecture
        

---

## Tech Stack

- **.NET 9**, ASP.NET Core Web API
    
- **EF Core** + SQL Server
    
- **Cookie Auth** + **Google OAuth**
    
- **Azure OpenAI** (via Foundry/Azure AI endpoint)
    
- Logging with `ILogger<>`, minimal dependencies
    

---

## Getting Started

### 1) Prerequisites

- .NET 9 SDK
    
- SQL Server (localdb or container)
    
- Google OAuth Client (Client ID & Secret)
    
- Azure OpenAI (endpoint + key)
    

### 2) Configuration

Create/update `appsettings.json`:

`{   "ConnectionStrings": {     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StudyEZ;Trusted_Connection=True;MultipleActiveResultSets=true"   },   "AzureOpenAI": {     "Endpoint": "https://<your-aoai>.openai.azure.com/",     "ApiKey": "<secret>",     "SimplifyDeployment": "gpt-5-mini",     "ExamDeployment": "gpt-4.1"   },   "Authentication": {     "Google": {       "ClientId": "<google-client-id>",       "ClientSecret": "<google-client-secret>"     }   },   "AllowedHosts": "*" }`

### 3) Database

`dotnet ef database update # (or: dotnet ef migrations add Init && dotnet ef database update)`

### 4) CORS (local dev)

Backend: allow the frontend origin (`https://localhost:3000`).

`builder.Services.AddCors(opt => {     opt.AddPolicy("fe", p => p         .WithOrigins("https://localhost:3000")         .AllowAnyHeader()         .AllowAnyMethod()         .AllowCredentials()); }); app.UseCors("fe");`

### 5) Run

`dotnet run`

- Swagger UI: `https://localhost:7097/swagger`
    
- Frontend (when ready): `https://localhost:3000`
    

> Dev seed: the app seeds a test user in Development if not present. You can also pass `?asUser=<guid>&asRole=Admin` to simulate claims while wiring things up.

---

## High-Level Flow

1. **Login**  
    `GET /api/auth/login` → Google consent → callback → cookie issued.  
    On first login, `UserService.UpsertFromGoogleAsync` creates/updates the user row.
    
2. **Courses**
    
    - `POST /api/courses` (owner or admin)
        
    - `PUT /api/courses/{id}` / `DELETE ...` (soft delete by default, hard w/ `?hard=true`)
        
    - `GET /api/courses/{id}`, `GET /api/courses/me`
        
3. **Modules**
    
    - `POST /api/modules` create (title, order, original content)
        
    - `PUT /api/modules/{id}`, `DELETE ...` / `POST /api/modules/{id}/restore`
        
    - `POST /api/modules/{id}/simplify` → calls **Azure OpenAI** → stores `SimplifiedContent`
        
4. **Exams**
    
    - `POST /api/exams` (generate from course modules)
        
    - `GET /api/exams/{id}/start` → returns ordered questions + options
        
    - `POST /api/exams/submit` → grades + creates `ExamResult` + module breakdown
        
    - `GET /api/results/{id}` or by user/exam for history
        
5. **Errors**  
    Domain exceptions (`NotFound`, `Conflict`, `Forbidden`, `Validation`) are thrown in services and mapped by middleware to consistent `ProblemDetails` responses.

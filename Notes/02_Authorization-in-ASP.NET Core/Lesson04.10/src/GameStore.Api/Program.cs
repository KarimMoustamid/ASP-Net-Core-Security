using GameStore.Api.Data;
using GameStore.Api.Features.Baskets;
using GameStore.Api.Features.Baskets.Authorization;
using GameStore.Api.Features.Games;
using GameStore.Api.Features.Genres;
using GameStore.Api.Shared.Authorization;
using GameStore.Api.Shared.ErrorHandling;
using GameStore.Api.Shared.FileUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;

// ============================================================================
// APPLICATION CONFIGURATION - Part 1: Build the Service Container
// ============================================================================
// This section configures all the services (dependencies) that the application
// will need. Think of this as registering all the "tools" before building them.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// === ERROR HANDLING CONFIGURATION ===
// Register global exception handler to provide consistent error responses
builder.Services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();

// === DATABASE CONFIGURATION ===
// Configure SQLite database connection
// Retrieves connection string from appsettings.json and registers GameStoreContext
var connString = builder.Configuration.GetConnectionString("GameStore");
builder.Services.AddSqlite<GameStoreContext>(connString);

// === HTTP LOGGING CONFIGURATION ===
// Configure structured logging for HTTP requests/responses
// Logs: HTTP method, request path, response status code, and request duration
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

// === SWAGGER DOCUMENTATION CONFIGURATION ===
// Enable API documentation discovery and UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === FILE UPLOAD CONFIGURATION ===
// Register HTTP context accessor for accessing request context
// Register FileUploader singleton for handling file operations
builder.Services.AddHttpContextAccessor()
                .AddSingleton<FileUploader>();

// === AUTHENTICATION CONFIGURATION ===
// Register JWT Bearer authentication scheme
// This enables the API to validate JWT tokens from requests
builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    // Don't map inbound claims to .NET claim types
                    // This preserves original JWT claim names (e.g., "sub" instead of NameIdentifier)
                    options.MapInboundClaims = false;
                    
                    // Map JWT "role" claim to ClaimTypes.Role
                    // This allows role-based authorization checks
                    options.TokenValidationParameters.RoleClaimType = "role";
                });

// === AUTHORIZATION CONFIGURATION ===
// Register authorization policies (UserAccess, AdminAccess)
// This extension method configures fallback policy requiring "gamestore_api.all" scope
builder.AddGameStoreAuthorization();

// === RESOURCE-BASED AUTHORIZATION CONFIGURATION ===
// Register BasketAuthorizationHandler for basket ownership checks
// This handler evaluates the OwnerOrAdminRequirement for basket resources
builder.Services.AddSingleton<IAuthorizationHandler, BasketAuthorizationHandler>();

// ============================================================================
// APPLICATION PIPELINE CONFIGURATION - Part 2: Build and Configure the App
// ============================================================================
// This section configures the HTTP request/response middleware pipeline.
// The order matters! Middleware is executed in the order it's added.
// ============================================================================

var app = builder.Build();

// === STATIC FILES MIDDLEWARE ===
// Serve static files (CSS, JavaScript, images) from wwwroot directory
app.UseStaticFiles();

// === AUTHORIZATION MIDDLEWARE ===
// This middleware enforces authorization policies and [Authorize] attributes
// MUST come after UseAuthentication() in a real app (though it's implicit here)
app.UseAuthorization();

// === ENDPOINT MAPPING ===
// Register all REST API endpoints from feature modules
app.MapGames();      // Maps /games endpoints
app.MapGenres();     // Maps /genres endpoints
app.MapBaskets();    // Maps /baskets endpoints (includes authorization)

// === HTTP LOGGING MIDDLEWARE ===
// Enable structured logging of HTTP requests/responses
app.UseHttpLogging();

// === SWAGGER UI CONFIGURATION ===
// Only show Swagger in development environment for testing
// In production, only the actual API is available
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
else
{
    // In production, use centralized exception handler
    app.UseExceptionHandler();
}

// === STATUS CODE PAGES MIDDLEWARE ===
// Provide user-friendly error pages for HTTP error status codes (404, 500, etc.)
app.UseStatusCodePages();

// === DATABASE INITIALIZATION ===
// Run database migrations and seed initial data
await app.InitializeDbAsync();

// === START THE APPLICATION ===
// Build and run the web server, listening for incoming HTTP requests
app.Run();
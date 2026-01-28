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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails()
                .AddExceptionHandler<GlobalExceptionHandler>();

var connString = builder.Configuration.GetConnectionString("GameStore");
builder.Services.AddSqlite<GameStoreContext>(connString);

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor()
                .AddSingleton<FileUploader>();

// Configure authentication with Keycloak as the default scheme.
builder.Services.AddAuthentication(Schemes.Keycloak)
                // Default JWT bearer configuration for the API.
                .AddJwtBearer(options =>
                {
                    // Keep claims in their original form from the token.
                    options.MapInboundClaims = false;
                    // Map role claims to ClaimTypes.Role for [Authorize(Roles = ...)].
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                })
                // Named scheme explicitly targeting Keycloak as the authority.
                .AddJwtBearer(Schemes.Keycloak, options =>
                {
                    // Keycloak realm issuing the tokens.
                    options.Authority = "http://localhost:8080/realms/gamestore";
                    // Expected audience in the access token.
                    options.Audience = "gamestore-api";
                    // Keep claims in their original form from the token.
                    options.MapInboundClaims = false;
                    // Map role claims to ClaimTypes.Role for [Authorize(Roles = ...)].
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                });

builder.AddGameStoreAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, BasketAuthorizationHandler>();

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthorization();

app.MapGames();
app.MapGenres();
app.MapBaskets();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

await app.InitializeDbAsync();

app.Run();
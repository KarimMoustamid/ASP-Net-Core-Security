using GameStore.Api.Data;
using GameStore.Api.Features.Baskets;
using GameStore.Api.Features.Baskets.Authorization;
using GameStore.Api.Features.Games;
using GameStore.Api.Features.Genres;
using GameStore.Api.Shared.Authorization;
using GameStore.Api.Shared.ErrorHandling;
using GameStore.Api.Shared.FileUpload;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddAuthentication(Schemes.Keycloak)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                })
                .AddJwtBearer(Schemes.Keycloak, options =>
                {
                    options.Authority = "http://localhost:8080/realms/gamestore";
                    options.Audience = "gamestore-api";
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
                    options.RequireHttpsMetadata = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var claims = context.Principal?.Claims;

                            if (claims is null)
                            {
                                return Task.CompletedTask;
                            }

                            var logger = context.HttpContext
                                                .RequestServices
                                                .GetRequiredService<ILogger<Program>>();

                            foreach (var claim in claims)
                            {
                                logger.LogInformation("Claim: {ClaimType}, Value: {ClaimValue}",
                                                      claim.Type,
                                                      claim.Value);
                            }

                            return Task.CompletedTask;
                        }
                    };
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
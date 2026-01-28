namespace GameStore.Api.Shared.Authorization;

public static class AuthorizationExtensions
{
    private const string ApiAccessScope = "gamestore_api.all";

    public static IHostApplicationBuilder AddGameStoreAuthorization(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
                        .AddFallbackPolicy(Policies.UserAccess, authBuilder =>
                        {
                            authBuilder.RequireClaim(ClaimTypes.Scope, ApiAccessScope);
                        })
                        .AddPolicy(Policies.AdminAccess, authBuilder =>
                        {
                            authBuilder.RequireClaim(ClaimTypes.Scope, ApiAccessScope);
                            authBuilder.RequireRole(Roles.Admin);
                        });

        return builder;
    }
}

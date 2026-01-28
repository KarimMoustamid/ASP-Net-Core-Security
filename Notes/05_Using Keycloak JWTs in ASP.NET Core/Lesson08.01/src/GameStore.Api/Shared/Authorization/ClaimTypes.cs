namespace GameStore.Api.Shared.Authorization;

/// <summary>
/// Defines claim type names used when reading JWTs from Keycloak.
/// </summary>
public static class ClaimTypes
{
    /// <summary>
    /// The claim type used by Keycloak to store role names.
    /// </summary>
    public const string Role = "role";
}

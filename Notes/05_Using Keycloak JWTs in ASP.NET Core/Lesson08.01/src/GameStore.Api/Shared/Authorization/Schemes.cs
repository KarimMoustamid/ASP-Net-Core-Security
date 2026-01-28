namespace GameStore.Api.Shared.Authorization;

/// <summary>
/// Defines authentication scheme names used across the API.
/// </summary>
public static class Schemes
{
    /// <summary>
    /// The Keycloak JWT bearer authentication scheme name.
    /// </summary>
    public const string Keycloak = nameof(Keycloak);
}

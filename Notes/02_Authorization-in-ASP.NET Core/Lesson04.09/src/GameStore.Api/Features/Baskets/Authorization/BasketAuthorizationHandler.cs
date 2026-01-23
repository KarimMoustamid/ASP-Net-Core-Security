using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameStore.Api.Models;
using GameStore.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace GameStore.Api.Features.Baskets.Authorization;

/// <summary>
/// Resource-based authorization handler for basket access control.
/// 
/// An authorization handler is a class responsible for evaluating whether 
/// a requirement is satisfied by examining the user's claims and properties.
/// 
/// A resource-based handler is an authorization handler that specifies both:
/// - A requirement (OwnerOrAdminRequirement)
/// - A resource type (CustomerBasket)
/// 
/// This handler ensures users can only access their own baskets, while admins
/// can access any basket.
/// </summary>
public class BasketAuthorizationHandler
    : AuthorizationHandler<OwnerOrAdminRequirement, CustomerBasket>
{
    /// <summary>
    /// Evaluates whether the current user has authorization to access the specified basket.
    /// 
    /// This method is invoked by ASP.NET Core's authorization system when a resource-based
    /// authorization check is performed. It implements the core logic for determining if
    /// the user satisfies the OwnerOrAdminRequirement for the given basket.
    /// </summary>
    /// <param name="context">
    /// The authorization context containing:
    /// - User: The current ClaimsPrincipal with user claims (sub, role, scope, etc.)
    /// - Methods: context.Succeed() to grant access, context.Fail() to deny access
    /// </param>
    /// <param name="requirement">
    /// The authorization requirement being evaluated. In this case, it's a marker
    /// requirement (OwnerOrAdminRequirement) with no specific data—the logic is 
    /// implemented in this handler method.
    /// </param>
    /// <param name="resource">
    /// The CustomerBasket resource being accessed. Contains:
    /// - Id: The basket/customer ID
    /// - Items: Collection of BasketItem objects
    /// </param>
    /// <returns>Completed task after authorization evaluation is complete</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement,
        CustomerBasket resource)
    {
        // Extract the user's unique identifier from the JWT 'sub' (subject) claim.
        // The 'sub' claim is a standard JWT claim that identifies the authenticated user.
        // Example value: "550e8400-e29b-41d4-a716-446655440000"
        var currentUserId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        // If the user has no 'sub' claim or it's null/empty, they are not properly authenticated.
        // Return without calling context.Succeed() to implicitly deny access.
        // This handles edge cases like malformed tokens or missing required claims.
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Task.CompletedTask;
        }

        // Authorization logic: Grant access if EITHER condition is true:
        // 
        // Condition 1: Ownership check
        //   - Parse the string currentUserId into a Guid
        //   - Compare it with the basket's Id (which equals the owner's ID)
        //   - If they match, the user is the basket owner
        //
        // Condition 2: Admin override
        //   - Check if the user has the "Admin" role
        //   - Admins can access any basket regardless of ownership
        //
        // If either condition is true, call context.Succeed() to grant access.
        if (Guid.Parse(currentUserId) == resource.Id
            || context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }

        // Return the completed task. Note: We don't explicitly call context.Fail()
        // if both conditions are false. ASP.NET Core assumes failure if Success() 
        // was never called, which is the default behavior for denied access.
        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement marker for basket ownership or admin access.
/// 
/// An authorization requirement is a collection of data parameters that a policy
/// can use to evaluate the current user principal.
/// 
/// This particular requirement has no data properties—it's a marker that says:
/// "Check if the user is the basket owner OR has the Admin role"
/// 
/// The actual evaluation logic is implemented in BasketAuthorizationHandler.
/// </summary>
public class OwnerOrAdminRequirement : IAuthorizationRequirement { }
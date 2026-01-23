using System.Security.Claims;
using GameStore.Api.Data;
using GameStore.Api.Features.Baskets.Authorization;
using GameStore.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Features.Baskets.GetBasket;

/// <summary>
/// Endpoint handler for retrieving a customer's shopping basket.
/// 
/// This endpoint demonstrates resource-based authorization:
/// - Users can only view their own basket (verified by matching user ID to basket ID)
/// - Admins can view any customer's basket
/// 
/// Route: GET /baskets/{userId}
/// Returns: BasketDto with items and total amount, or 403 Forbidden if unauthorized
/// </summary>
public static class GetBasketEndpoint
{
    /// <summary>
    /// Maps the GET /baskets/{userId} endpoint.
    /// 
    /// This method configures a REST API endpoint that retrieves a specific customer's basket.
    /// The endpoint uses resource-based authorization to ensure users can only access their own
    /// basket or admins can access any basket.
    /// </summary>
    public static void MapGetBasket(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{userId}", async (
            // The customer ID whose basket to retrieve
            Guid userId,
            // Database context for querying basket data
            GameStoreContext dbContext,
            // Authorization service for performing resource-based authorization checks
            IAuthorizationService authorizationService,
            // The current authenticated user's claims (extracted from JWT token)
            ClaimsPrincipal user
        ) =>
        {
            // Validate that a valid userId was provided
            // Return 400 Bad Request if userId is the default Guid (all zeros)
            if (userId == Guid.Empty)
            {
                return Results.BadRequest();
            }

            // Query the database for the basket:
            // 1. Find the basket with matching ID
            // 2. Include all BasketItems in that basket
            // 3. Include the Game details for each BasketItem (for name, price, image)
            // 4. If basket doesn't exist, create an empty new one with just the userId
            //    (This allows viewing an empty cart for new customers)
            var basket = await dbContext.Baskets
                                        .Include(basket => basket.Items)
                                        .ThenInclude(item => item.Game)
                                        .FirstOrDefaultAsync(
                                            basket => basket.Id == userId)
                                            ?? new() { Id = userId };

            // Perform resource-based authorization:
            // - Check if the current user satisfies the OwnerOrAdminRequirement for this basket
            // - The BasketAuthorizationHandler will verify:
            //   1. User is the basket owner (user ID == basket ID), OR
            //   2. User has the Admin role
            var authResult = await authorizationService.AuthorizeAsync(
                user,
                basket,
                new OwnerOrAdminRequirement()
            );

            // If authorization failed, return 403 Forbidden
            // This prevents users from viewing other customers' baskets
            if (!authResult.Succeeded)
            {
                return Results.Forbid();
            }

            // Transform the basket data into a DTO (Data Transfer Object) for the response
            // - Extract basket ID and customer ID
            // - Convert each BasketItem to a BasketItemDto with game details
            // - Sort items alphabetically by game name for consistent ordering
            var dto = new BasketDto(
                basket.Id,
                basket.Items.Select(item => new BasketItemDto(
                    item.GameId,
                    item.Game!.Name,
                    item.Game!.Price,
                    item.Quantity,
                    item.Game!.ImageUri
                ))
                .OrderBy(item => item.Name));

            // Return the basket as a 200 OK response
            return Results.Ok(dto);
        });
    }
}

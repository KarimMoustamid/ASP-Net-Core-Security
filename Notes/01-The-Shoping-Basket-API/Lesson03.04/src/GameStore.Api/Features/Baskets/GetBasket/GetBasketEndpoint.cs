using GameStore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Features.Baskets.GetBasket;

public static class GetBasketEndpoint
{
    public static void MapGetBasket(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{userId}", async (
            Guid userId,
            GameStoreContext dbContext
        ) =>
        {
            if (userId == Guid.Empty)
            {
                return Results.BadRequest();
            }

            /*
            The null-coalescing expression ?? new() { Id = userId } means: if no basket is found for that userId (FirstOrDefaultAsync returns null), create a new empty Basket instance with its Id set to the requested userId. This prevents null handling later—basket is guaranteed non-null, so you can safely build the DTO and return an empty basket instead of a 404.
            */
            var basket = await dbContext.Baskets
                                        .Include(basket => basket.Items)
                                        .ThenInclude(item => item.Game)
                                        .FirstOrDefaultAsync(
                                            basket => basket.Id == userId) 
                                            ?? new() { Id = userId };
                                            

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

            return Results.Ok(dto);
        });
    }
}

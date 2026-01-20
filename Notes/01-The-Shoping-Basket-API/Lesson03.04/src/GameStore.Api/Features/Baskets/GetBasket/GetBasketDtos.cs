namespace GameStore.Api.Features.Baskets.GetBasket;

public record class BasketDto(
    Guid CustomerId,
    IEnumerable<BasketItemDto> Items
)
{
    // Computed total so callers get the basket cost without recalculating client-side
    public decimal TotalAmount => Items.Sum(item => item.Price * item.Quantity);
}

public record class BasketItemDto(
    Guid Id,
    string Name,
    decimal Price,
    int Quantity,
    string ImageUri
);


/*
A record’s primary constructor parameters define its core state, but you can still add members inside the body. Here, BasketDto has two positional parameters (CustomerId, Items) and then an extra computed property TotalAmount in the body. That property isn’t stored separately—it just derives from Items on access. So:

Primary ctor params become properties automatically.
Body members (like TotalAmount) are just additional properties/methods you define.
For records, this is common for computed values you don’t want as stored fields but want available to callers.
If the extra property feels odd, remember it isn’t part of the equality contract unless you explicitly include it; records’ value equality still keys off the primary constructor parameters by default.
*/
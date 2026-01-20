namespace GameStore.Api.Models;

public class BasketItem
{
    public Guid Id { get; set; }

    public Game? Game { get; set; }

    // Foreign Key: Best practice to include alongside navigation property for:
    // - Explicit relationship mapping for EF Core
    // - Query efficiency without loading full Game object
    // - Database integrity constraints
    // - Clear nullable/required semantics (FK is required, navigation is optional)
    // - EF convention recognition ({EntityName}Id pattern)
    public Guid GameId { get; set; }

    public int Quantity { get; set; }

    public Guid CustomerBasketId { get; set; }
}

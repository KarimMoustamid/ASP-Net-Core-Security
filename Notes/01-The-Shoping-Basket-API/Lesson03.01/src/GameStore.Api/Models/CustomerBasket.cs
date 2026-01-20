namespace GameStore.Api.Models;

public class CustomerBasket
{
    public Guid Id { get; set; } // the basket Id is the same as the customer Id

    public List<BasketItem> Items { get; set; } = [];
}

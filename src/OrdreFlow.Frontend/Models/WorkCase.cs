namespace OrdreFlow.Frontend.Models;

// A work case groups the orders (cases) that time can be registered on.
public record WorkCase(int Id, string Name, IReadOnlyList<Order> Orders);

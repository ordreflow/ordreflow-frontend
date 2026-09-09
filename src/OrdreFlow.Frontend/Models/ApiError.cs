namespace OrdreFlow.Frontend.Models;

public record ApiError(string Code, string Message, string? Type = null);

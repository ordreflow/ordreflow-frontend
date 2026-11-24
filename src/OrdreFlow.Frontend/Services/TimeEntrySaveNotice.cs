namespace OrdreFlow.Frontend.Services;

// Carries a success message across the redirect, then clears it so it cannot reappear later.
public sealed class TimeEntrySaveNotice
{
    private string? message;

    public void Set(string value) => message = value;

    public string? Take()
    {
        var value = message;
        message = null;
        return value;
    }
}

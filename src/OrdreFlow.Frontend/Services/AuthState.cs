using System.Text.Json;
using Microsoft.JSInterop;
using OrdreFlow.Frontend.Models;

namespace OrdreFlow.Frontend.Services;

// Holds the signed-in session. The token is kept in localStorage so a reload on the phone does not log the user out.
public class AuthState(IJSRuntime jsRuntime)
{
    private const string StorageKey = "ordreflow.session";

    private Task? restoreTask;

    public string? AccessToken { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public UserResponse? User { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrEmpty(AccessToken) && ExpiresAt.ToUniversalTime() > DateTime.UtcNow;

    // Mirrors the backend's ManagerOrAdmin policy so the UI only offers actions the API will allow.
    public bool IsManagerOrAdmin =>
        string.Equals(User?.Role, "Manager", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(User?.Role, "Admin", StringComparison.OrdinalIgnoreCase);

    public event Action? Changed;

    // Safe to call from several components; the session is only read from storage once.
    public Task InitializeAsync() => restoreTask ??= RestoreAsync();

    public async Task SignInAsync(LoginResponse login)
    {
        AccessToken = login.AccessToken;
        ExpiresAt = login.ExpiresAt;
        User = null;
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task SetUserAsync(UserResponse user)
    {
        User = user;
        await PersistAsync();
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        AccessToken = null;
        ExpiresAt = default;
        User = null;
        await WriteStorageAsync(null);
        Changed?.Invoke();
    }

    private async Task RestoreAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            var session = json is null ? null : JsonSerializer.Deserialize<StoredSession>(json);
            if (session is not null && session.ExpiresAt.ToUniversalTime() > DateTime.UtcNow)
            {
                AccessToken = session.AccessToken;
                ExpiresAt = session.ExpiresAt;
                User = session.User;
            }
            else if (session is not null)
            {
                await WriteStorageAsync(null);
            }
        }
        catch (Exception ex) when (ex is JSException or JsonException or InvalidOperationException)
        {
            // Storage can be unavailable or hold garbage (private mode, edited by hand); start signed out.
        }

        Changed?.Invoke();
    }

    private Task PersistAsync() =>
        WriteStorageAsync(JsonSerializer.Serialize(new StoredSession(AccessToken!, ExpiresAt, User)));

    private async Task WriteStorageAsync(string? json)
    {
        try
        {
            if (json is null)
            {
                await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
            else
            {
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
            }
        }
        catch (JSException)
        {
            // The session still works in memory for this page load.
        }
    }

    private sealed record StoredSession(string AccessToken, DateTime ExpiresAt, UserResponse? User);
}

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrdreFlow.Frontend;
using OrdreFlow.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    // Fail fast with a clear message instead of hanging on "Saving..." when the API is unreachable.
    Timeout = TimeSpan.FromSeconds(15)
});
builder.Services.AddScoped<ITimeEntriesApiClient, TimeEntriesApiClient>();
builder.Services.AddScoped<IWorkCasesProvider, DummyWorkCasesProvider>();
builder.Services.AddScoped<SavedTimeEntriesState>();

await builder.Build().RunAsync();

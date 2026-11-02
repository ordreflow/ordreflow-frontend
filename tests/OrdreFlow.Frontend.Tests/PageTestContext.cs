using System.Net;
using System.Net.Http.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using OrdreFlow.Frontend.Models;
using OrdreFlow.Frontend.Services;

namespace OrdreFlow.Frontend.Tests;

public abstract class PageTestContext : TestContext
{
    protected readonly UserResponse Manager = new(
        Guid.NewGuid(), "Manager", "manager@example.com", "Manager", "Active", DateTime.UtcNow);

    protected readonly TestApi Api = new();

    protected PageTestContext()
    {
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
        Services.AddSingleton(new HttpClient(Api) { BaseAddress = new Uri("https://localhost/") });
        Services.AddScoped<AuthState>();
        Services.AddScoped<UsersApiClient>();
        Services.AddScoped<OrdersApiClient>();
        Api.Json("api/users/me", Manager);
    }

    protected sealed class TestApi : HttpMessageHandler
    {
        private readonly Dictionary<(HttpMethod, string), Func<HttpResponseMessage>> responses = new();
        public List<(HttpMethod Method, string Path, string? Body)> Requests { get; } = [];

        // Full path with query string, for endpoints whose parameters are in the query.
        public List<string> RequestUris { get; } = [];

        public void Json<T>(string path, T value) =>
            responses[(HttpMethod.Get, path)] = () => new(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(value)
            };

        public void File(string path, byte[] content, string contentType, string fileName) =>
            responses[(HttpMethod.Get, path)] = () =>
            {
                var file = new ByteArrayContent(content);
                file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                file.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                return new(HttpStatusCode.OK) { Content = file };
            };

        public void Accept(HttpMethod method, string path) =>
            responses[(method, path)] = () => new(HttpStatusCode.NoContent);

        public void Fail(HttpMethod method, string path, HttpStatusCode status, ApiError error) =>
            responses[(method, path)] = () => new(status)
            {
                Content = JsonContent.Create(new[] { error })
            };

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath.TrimStart('/');
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.Method, path, body));
            RequestUris.Add(request.RequestUri.PathAndQuery);
            if (!responses.TryGetValue((request.Method, path), out var respond))
                throw new InvalidOperationException($"Unexpected API request: {request.Method} {path}");
            return respond();
        }
    }
}

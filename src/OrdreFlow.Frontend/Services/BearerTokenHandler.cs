using System.Net;
using System.Net.Http.Headers;

namespace OrdreFlow.Frontend.Services;

// Adds the access token to every API call and signs the user out when the API says the token is no longer valid.
public class BearerTokenHandler(AuthState authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenSent = false;
        if (authState.IsAuthenticated && request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
            tokenSent = true;
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (tokenSent && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await authState.SignOutAsync();
        }

        return response;
    }
}

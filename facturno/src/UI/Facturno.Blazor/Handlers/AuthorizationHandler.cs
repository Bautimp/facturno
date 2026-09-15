using System.Net;
using System.Net.Http.Headers;
using Facturno.Blazor.Services;

namespace Facturno.Blazor.Handlers;

public class AuthorizationHandler : DelegatingHandler
{
    private readonly LocalStorageService _localStorageService;
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationHandler(LocalStorageService localStorageService, IServiceProvider serviceProvider)
    {
        _localStorageService = localStorageService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorageService.GetItemAsync("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var authStateProvider = _serviceProvider.GetService(typeof(CustomAuthStateProvider)) as CustomAuthStateProvider;
            if (authStateProvider != null)
            {
                await authStateProvider.MarkUserAsLoggedOut();
            }
        }

        return response;
    }
}

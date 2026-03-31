using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace POS.Frontend.Services.Auth;

public class TokenInterceptor : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceProvider _serviceProvider;

    public TokenInterceptor(ILocalStorageService localStorage, IServiceProvider serviceProvider)
    {
        _localStorage = localStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var absPath = request.RequestUri?.AbsolutePath;
        bool isAuthRequest = absPath?.Contains("/api/auth/") ?? false;

        if (!isAuthRequest)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (!isAuthRequest && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var newToken = await authService.RefreshTokenAsync();

            if (!string.IsNullOrEmpty(newToken))
            {
                // Clone the request if necessary, but for simple Bearer change we can just update header and retry
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                
                // We need to be careful with retrying if the content was already sent.
                // For GET/DELETE it's fine. For POST/PUT with stream it's tricky.
                // However, in Blazor Wasm content is usually buffered.
                
                return await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}

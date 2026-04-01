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
                var requestClone = await CloneRequestAsync(request);
                requestClone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                return await base.SendAsync(requestClone, cancellationToken);
            }
        }

        return response;
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        
        // Copy content
        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            
            if (request.Content.Headers != null)
            {
                foreach (var h in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }
            }
        }
        
        clone.Version = request.Version;
        
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
        foreach (var prop in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
        }
#else
        foreach (var prop in request.Properties)
        {
            clone.Properties.Add(prop.Key, prop.Value);
        }
#endif

        return clone;
    }
}

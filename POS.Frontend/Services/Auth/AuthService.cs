using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using POS.Shared.Models;

namespace POS.Frontend.Services.Auth;
public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<string?> RefreshTokenAsync();
}
public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", request);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                return Result<AuthResponse>.Success(result);
            }
        }

        var error = await response.Content.ReadAsStringAsync();
        return Result<AuthResponse>.Failure(error ?? "Login failed.");
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await _http.PostAsJsonAsync("/api/auth/revoke-token", new RefreshTokenRequest { Token = refreshToken });
            }
            catch { /* Ignore network errors on logout */ }
        }

        // Clear all possible token keys
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("token"); // Old key cleanup
        
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    private Task<string?>? _refreshTokenTask;

    public Task<string?> RefreshTokenAsync()
    {
        if (_refreshTokenTask == null)
        {
            _refreshTokenTask = RefreshTokenInternalAsync();
        }
        return _refreshTokenTask;
    }

    private async Task<string?> RefreshTokenInternalAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return null;

            var response = await _http.PostAsJsonAsync("/api/auth/refresh-token", new RefreshTokenRequest 
            { 
                Token = refreshToken 
            });

            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                await LogoutAsync();
                return null;
            }

            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
            
            return result.Token;
        }
        finally
        {
            _refreshTokenTask = null; // Clear the task so future calls get a new refresh
        }
    }
}

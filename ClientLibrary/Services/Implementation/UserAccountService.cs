using System.Net.Http.Json;
using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace ClientLibrary.Services.Implementation;

public class UserAccountService : IUserAccountService
{
    private readonly GetHttpClient _getHttpClient;
    private readonly ILogger<UserAccountService> _logger;

    private const string AuthUrl = "api/authentication";

    public UserAccountService(GetHttpClient getHttpClient,ILogger<UserAccountService> logger)
    {
        _getHttpClient = getHttpClient;
        _logger = logger;
    }

    public async Task<GeneralResponse> CreateAsync(Register user)
    {
        ArgumentNullException.ThrowIfNull(user);
        try
        {
            var httpClient = await _getHttpClient.GetPrivateHttpClient();
            using var response = await httpClient.PostAsJsonAsync($"{AuthUrl}/register", user);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("User registration failed. StatusCode={StatusCode}",response.StatusCode);
                return new GeneralResponse(false, "Registration failed");
            }

            return await response.Content.ReadFromJsonAsync<GeneralResponse>()
                ?? new GeneralResponse(false, "Invalid server response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            return new GeneralResponse(false, "Unexpected error occurred");
        }
    }

    public async Task<LoginResponse> SignInAsync(Login user)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            var httpClient = _getHttpClient.GetPublicHttpClient();
            using var response = await httpClient.PostAsJsonAsync($"{AuthUrl}/login", user);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Login failed for {Email}. StatusCode={StatusCode}",
                    user.Email,
                    response.StatusCode);

                return new LoginResponse(false, "Invalid credentials");
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>()
                ?? new LoginResponse(false, "Invalid login response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", user.Email);
            return new LoginResponse(false, "Authentication error");
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<WeatherForecast[]> GetWeatherForecast()
    {
        try
        {
            var httpClient = await _getHttpClient.GetPrivateHttpClient();
            return await httpClient.GetFromJsonAsync<WeatherForecast[]>("api/weatherforecast") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather forecast");
            return [];
        }
    }
}

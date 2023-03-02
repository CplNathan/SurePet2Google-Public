// Copyright (c) Nathan Ford. All rights reserved. RequestService.cs

using SurePet2Google.Blazor.Shared;
using System.Net.Http.Json;

public delegate void AuthenticationEventHandler(object? sender, bool authenticated);

public interface IAuthService
{
    public static AuthenticationEventHandler? OnAuthentication;
    public Task<RegisterResponse?> Register(RegisterModel registerRequest);
}

public class RequestService : IAuthService
{
    private readonly HttpClient _httpClient;

    public RequestService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<RegisterResponse?> Register(RegisterModel registerRequest)
    {
        try
        {
            HttpResponseMessage userResult = await this._httpClient.PostAsJsonAsync<RegisterModel>("/surepet/API/Authentication/Register", registerRequest);
            if (userResult.IsSuccessStatusCode)
            {
                RegisterResponse? result = await userResult.Content.ReadFromJsonAsync<RegisterResponse>();
                return result;
            }
            else
            {
                return new();
            }
        }
        catch
        {
            return new();
        }
    }
}

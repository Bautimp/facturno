using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.Blazor.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorageService;
    private readonly AuthenticationState _anonymousState;

    public CustomAuthStateProvider(LocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
        _anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userJson = await _localStorageService.GetItemAsync("authUser");
            if (string.IsNullOrWhiteSpace(userJson))
            {
                return _anonymousState;
            }

            var userDto = JsonSerializer.Deserialize<UsuarioAuthResponseDto>(userJson);
            if (userDto == null || string.IsNullOrWhiteSpace(userDto.TokenJwt))
            {
                return _anonymousState;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userDto.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, userDto.NombreCompleto),
                new Claim(ClaimTypes.Email, userDto.Email),
                new Claim(ClaimTypes.Role, userDto.Rol.ToString())
            };

            var identity = new ClaimsIdentity(claims, "JwtAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationState(userPrincipal);
        }
        catch
        {
            return _anonymousState;
        }
    }

    public async Task MarkUserAsAuthenticated(UsuarioAuthResponseDto userDto)
    {
        var userJson = JsonSerializer.Serialize(userDto);
        await _localStorageService.SetItemAsync("authToken", userDto.TokenJwt);
        await _localStorageService.SetItemAsync("authUser", userJson);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userDto.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, userDto.NombreCompleto),
            new Claim(ClaimTypes.Email, userDto.Email),
            new Claim(ClaimTypes.Role, userDto.Rol.ToString())
        };

        var identity = new ClaimsIdentity(claims, "JwtAuthType");
        var userPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorageService.RemoveItemAsync("authToken");
        await _localStorageService.RemoveItemAsync("authUser");

        NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
    }
}

using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.DTOs;
using Facturno.Shared.Utils;
using Facturno.Blazor.Services;

namespace Facturno.Blazor.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    [Inject]
    public CustomAuthStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected UsuarioLoginDto loginModel = new();
    protected string? ErrorMessage;
    protected bool isSubmitting = false;
    protected bool mostrarPassword = false;

    protected void ToggleMostrarPassword()
    {
        mostrarPassword = !mostrarPassword;
    }

    protected void NormalizarCorreo()
    {
        if (!string.IsNullOrWhiteSpace(loginModel.Correo))
        {
            loginModel.Correo = loginModel.Correo.Trim().ToLowerInvariant();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            NavigationManager.NavigateTo("/agenda", replace: true);
        }
    }

    protected async Task HandleLogin()
    {
        NormalizarCorreo();
        ErrorMessage = null;
        isSubmitting = true;

        try
        {
            var response = await Http.PostAsJsonAsync("api/auth/login", loginModel);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<UsuarioAuthResponseDto>(response, "Credenciales inválidas. Por favor intenta de nuevo.");

            if (exito && datos != null)
            {
                await AuthStateProvider.MarkUserAsAuthenticated(datos);
                NavigationManager.NavigateTo("/agenda");
            }
            else
            {
                ErrorMessage = mensaje;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al conectar con el servidor: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }
}

using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.DTOs;
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

    protected async Task HandleLogin()
    {
        ErrorMessage = null;
        isSubmitting = true;

        try
        {
            var response = await Http.PostAsJsonAsync("api/auth/login", loginModel);
            var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioAuthResponseDto>>();

            if (apiResult != null && apiResult.Exito && apiResult.Datos != null)
            {
                await AuthStateProvider.MarkUserAsAuthenticated(apiResult.Datos);
                NavigationManager.NavigateTo("/agenda");
            }
            else
            {
                ErrorMessage = apiResult?.Mensaje ?? "Credenciales inválidas. Por favor intenta de nuevo.";
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

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Facturno.Blazor.Services;

namespace Facturno.Blazor.Layout;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    public CustomAuthStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected string GetUserRole(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value ?? "Usuario";
    }

    protected async Task Logout()
    {
        await AuthStateProvider.MarkUserAsLoggedOut();
        NavigationManager.NavigateTo("/login");
    }
}

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

    protected string GetUserInitials(ClaimsPrincipal user)
    {
        var name = user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(name)) return "U";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }
        return name[..Math.Min(2, name.Length)].ToUpper();
    }

    protected async Task Logout()
    {
        await AuthStateProvider.MarkUserAsLoggedOut();
        NavigationManager.NavigateTo("/login");
    }
}

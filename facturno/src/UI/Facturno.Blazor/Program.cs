using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Facturno.Blazor;
using Facturno.Blazor.Services;
using Facturno.Blazor.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registrar Servicios de Autenticación y LocalStorage
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AuthorizationHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

// Configurar HttpClient apuntando a Facturno.API con el interceptor AuthorizationHandler
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5082";

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseAddress)
    };
});

await builder.Build().RunAsync();

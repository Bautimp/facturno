using Microsoft.Extensions.Caching.Memory;
using Facturno.Shared.Interfaces;
using Facturno.API.Services;
using Facturno.Infrastructure.Supabase.Repositories;
using Supabase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar soporte para Controladores RESTful
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 2. Configurar el Cliente de Supabase en IoC
var supabaseUrl = builder.Configuration["Supabase:Url"] 
    ?? Environment.GetEnvironmentVariable("SUPABASE_URL") 
    ?? string.Empty;

var supabaseKey = builder.Configuration["Supabase:Key"] 
    ?? Environment.GetEnvironmentVariable("SUPABASE_KEY") 
    ?? string.Empty;

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    }));

// 3. Registrar Repositorios de Infraestructura
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IObraSocialRepository, ObraSocialRepository>();

// 4. Registrar Servicios de Lógica de Negocio y PDF
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IFacturaPdfService, Facturno.Infrastructure.Services.FacturaPdfService>();

builder.Services.AddMemoryCache();

// 5. Configurar Autenticación y Autorización (Supabase JWKS / ES256)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{supabaseUrl}/auth/v1";
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl}/auth/v1",
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                         ?? context.Principal?.FindFirst("email")?.Value;

                if (!string.IsNullOrEmpty(email))
                {
                    var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheKey = $"user_role_{email}";

                    if (!cache.TryGetValue(cacheKey, out object? cachedObj) || cachedObj is not string rolString)
                    {
                        var repo = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
                        var usuario = await repo.ObtenerPorCorreoAsync(email);
                        if (usuario != null && usuario.Activo)
                        {
                            rolString = usuario.Rol.ToString();
                            cache.Set(cacheKey, rolString, TimeSpan.FromMinutes(5));
                        }
                        else
                        {
                            rolString = string.Empty;
                        }
                    }

                    if (!string.IsNullOrEmpty(rolString))
                    {
                        var identity = (System.Security.Claims.ClaimsIdentity?)context.Principal?.Identity;
                        if (identity != null)
                        {
                            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, rolString));
                        }
                    }
                }
            }
        };
    });


builder.Services.AddAuthorization();

// 6. Configurar CORS para Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorOrigin", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Pipeline de Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

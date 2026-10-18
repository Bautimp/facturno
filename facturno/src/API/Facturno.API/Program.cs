using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Facturno.Shared.Interfaces;
using Facturno.API.Services;
using Facturno.Infrastructure.PocketBase;
using Facturno.Infrastructure.PocketBase.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Soporte para Controladores RESTful y OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 2. Configurar PocketBase Client en IoC
builder.Services.AddHttpClient<PocketBaseClient>();

// 3. Registrar Repositorios de Infraestructura (PocketBase)
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IObraSocialRepository, ObraSocialRepository>();

// 4. Registrar Servicios de Lógica de Negocio, PDF y JWT
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IFacturaPdfService, Facturno.Infrastructure.Services.FacturaPdfService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddMemoryCache();

// 5. Configurar Autenticación JWT local simétrica (HMAC-SHA256)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "Facturno_Local_PocketBase_Migration_Super_Secret_Jwt_Signing_Key_2026";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Facturno.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Facturno.Clients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
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
                        var usuario = await repo.ObtenerPorCorreo(email);
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
                        if (identity != null && !identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role))
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

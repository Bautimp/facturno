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

// 5. Configurar Autenticación y Autorización
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false
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

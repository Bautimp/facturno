using Facturno.Shared.Interfaces;
using Facturno.Infrastructure.Supabase;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAR SUPABASE
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
// Opcional: configurar opciones adicionales del cliente
var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true }; 

// Inyectamos el cliente de Supabase como Singleton (una única conexión para toda la API)
builder.Services.AddSingleton(provider => new Supabase.Client(supabaseUrl!, supabaseKey!, options));

// 2. REGISTRAR LOS REPOSITORIOS
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IProfesionalRepository, ProfesionalRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Habilitar soporte para Controladores
builder.Services.AddControllers();

// Configuración de OpenAPI (Swagger) que viene por defecto
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapear las rutas de los controladores
app.MapControllers();

app.Run();
using System.Globalization;
using System.Text.Json;
using Facturno.Shared.DTOs;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Facturno.API.Services;

public class ConfiguracionService : IConfiguracionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfiguracionService> _logger;
    private readonly string _storageFilePath;
    private readonly object _lock = new();
    private ConfiguracionSistema _configuracion;

    public ConfiguracionService(IConfiguration configuration, ILogger<ConfiguracionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var baseDir = AppContext.BaseDirectory;
        _storageFilePath = Path.Combine(baseDir, "configuracion_sistema.json");

        _configuracion = CargarConfiguracionInicial();
    }

    public ConfiguracionSistema ObtenerConfiguracion()
    {
        lock (_lock)
        {
            return new ConfiguracionSistema
            {
                HoraInicioTurnos = _configuracion.HoraInicioTurnos,
                HoraFinTurnos = _configuracion.HoraFinTurnos,
                MaxPacientesPorDiaPorProfesional = _configuracion.MaxPacientesPorDiaPorProfesional,
                PrecioMaximoConsulta = _configuracion.PrecioMaximoConsulta,
                MinutosEntreTurnos = _configuracion.MinutosEntreTurnos
            };
        }
    }

    public async Task<ApiResponse<ConfiguracionSistema>> ActualizarConfiguracion(ConfiguracionSistemaDto dto)
    {
        if (!TimeOnly.TryParse(dto.HoraInicioTurnos, out var inicio) || !TimeOnly.TryParse(dto.HoraFinTurnos, out var fin))
        {
            return ApiResponse<ConfiguracionSistema>.Error("El formato de las horas debe ser HH:mm (ej: 08:00 y 20:00).");
        }

        if (inicio >= fin)
        {
            return ApiResponse<ConfiguracionSistema>.Error("La hora de inicio de turnos debe ser menor a la hora de finalización.");
        }

        if (dto.MaxPacientesPorDiaPorProfesional <= 0)
        {
            return ApiResponse<ConfiguracionSistema>.Error("La cantidad máxima de pacientes por día debe ser mayor a 0.");
        }

        if (dto.PrecioMaximoConsulta < 0)
        {
            return ApiResponse<ConfiguracionSistema>.Error("El precio máximo de consulta no puede ser negativo.");
        }

        if (dto.MinutosEntreTurnos <= 0 || dto.MinutosEntreTurnos > 480)
        {
            return ApiResponse<ConfiguracionSistema>.Error("El intervalo entre turnos debe ser entre 1 y 480 minutos.");
        }

        ConfiguracionSistema nuevaConfig;
        lock (_lock)
        {
            _configuracion.HoraInicioTurnos = dto.HoraInicioTurnos;
            _configuracion.HoraFinTurnos = dto.HoraFinTurnos;
            _configuracion.MaxPacientesPorDiaPorProfesional = dto.MaxPacientesPorDiaPorProfesional;
            _configuracion.PrecioMaximoConsulta = dto.PrecioMaximoConsulta;
            _configuracion.MinutosEntreTurnos = dto.MinutosEntreTurnos;

            nuevaConfig = ObtenerConfiguracion();
        }

        // Actualizar variables de proceso
        ActualizarVariablesDeEntorno(nuevaConfig);

        // Persistir en archivo JSON
        try
        {
            var json = JsonSerializer.Serialize(nuevaConfig, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_storageFilePath, json);
            _logger.LogInformation("Variables de configuración del sistema guardadas en {Path}", _storageFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo guardar la configuración en disco en {Path}", _storageFilePath);
        }

        return ApiResponse<ConfiguracionSistema>.Ok(nuevaConfig, "Configuración del sistema y variables actualizadas exitosamente.");
    }

    private ConfiguracionSistema CargarConfiguracionInicial()
    {
        var config = new ConfiguracionSistema();

        // 1. Intentar cargar desde archivo persistido si existe
        if (File.Exists(_storageFilePath))
        {
            try
            {
                var content = File.ReadAllText(_storageFilePath);
                var cargada = JsonSerializer.Deserialize<ConfiguracionSistema>(content);
                if (cargada != null)
                {
                    config = cargada;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al leer {Path}, usando valores predeterminados o de entorno.", _storageFilePath);
            }
        }

        // 2. Sobrescribir con Variables de Entorno del sistema si están definidas
        var envHoraInicio = Environment.GetEnvironmentVariable("FACTURNO_HORA_INICIO_TURNOS") 
            ?? _configuration["Configuracion:HoraInicioTurnos"];
        if (!string.IsNullOrWhiteSpace(envHoraInicio) && TimeOnly.TryParse(envHoraInicio, out _))
        {
            config.HoraInicioTurnos = envHoraInicio.Trim();
        }

        var envHoraFin = Environment.GetEnvironmentVariable("FACTURNO_HORA_FIN_TURNOS") 
            ?? _configuration["Configuracion:HoraFinTurnos"];
        if (!string.IsNullOrWhiteSpace(envHoraFin) && TimeOnly.TryParse(envHoraFin, out _))
        {
            config.HoraFinTurnos = envHoraFin.Trim();
        }

        var envMaxPacientes = Environment.GetEnvironmentVariable("FACTURNO_MAX_PACIENTES_DIA") 
            ?? _configuration["Configuracion:MaxPacientesPorDiaPorProfesional"];
        if (!string.IsNullOrWhiteSpace(envMaxPacientes) && int.TryParse(envMaxPacientes, out var maxPac) && maxPac > 0)
        {
            config.MaxPacientesPorDiaPorProfesional = maxPac;
        }

        var envPrecioMax = Environment.GetEnvironmentVariable("FACTURNO_PRECIO_MAXIMO_CONSULTA") 
            ?? _configuration["Configuracion:PrecioMaximoConsulta"];
        if (!string.IsNullOrWhiteSpace(envPrecioMax) && decimal.TryParse(envPrecioMax, NumberStyles.Any, CultureInfo.InvariantCulture, out var precioMax) && precioMax >= 0)
        {
            config.PrecioMaximoConsulta = precioMax;
        }

        var envMinutos = Environment.GetEnvironmentVariable("FACTURNO_MINUTOS_ENTRE_TURNOS") 
            ?? _configuration["Configuracion:MinutosEntreTurnos"];
        if (!string.IsNullOrWhiteSpace(envMinutos) && int.TryParse(envMinutos, out var minutos) && minutos > 0)
        {
            config.MinutosEntreTurnos = minutos;
        }

        ActualizarVariablesDeEntorno(config);
        return config;
    }

    private static void ActualizarVariablesDeEntorno(ConfiguracionSistema c)
    {
        Environment.SetEnvironmentVariable("FACTURNO_HORA_INICIO_TURNOS", c.HoraInicioTurnos);
        Environment.SetEnvironmentVariable("FACTURNO_HORA_FIN_TURNOS", c.HoraFinTurnos);
        Environment.SetEnvironmentVariable("FACTURNO_MAX_PACIENTES_DIA", c.MaxPacientesPorDiaPorProfesional.ToString());
        Environment.SetEnvironmentVariable("FACTURNO_PRECIO_MAXIMO_CONSULTA", c.PrecioMaximoConsulta.ToString(CultureInfo.InvariantCulture));
        Environment.SetEnvironmentVariable("FACTURNO_MINUTOS_ENTRE_TURNOS", c.MinutosEntreTurnos.ToString());
    }
}


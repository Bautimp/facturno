using Facturno.Shared.Enums;

namespace Facturno.Shared.DTOs;

// --- Turno DTOs ---
//Recibe el objeto del turno a crear
public class TurnoCreateDto
{
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public long IdPaciente { get; set; }
    public Guid IdProfesional { get; set; }
    public string? Observacion { get; set; }
}

//Recibe el objeto del turno a modificar
public class TurnoUpdateDto
{
    public long IdTurno { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public EstadoTurno Estado { get; set; }
    public string? Observacion { get; set; }
}

//Recibe el objeto del turno recurrente a crear
public class TurnoRecurrenteCreateDto
{
    public long IdPaciente { get; set; }
    public Guid IdProfesional { get; set; }
    public TimeOnly Hora { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Observacion { get; set; }
}

// --- Paciente DTOs ---
//Recibe el objeto del paciente a crear
public class PacienteCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public long? Telefono { get; set; }
    public string? NumDocumento { get; set; }
    public TipoDocumento? TipoDocumento { get; set; }
    public long IdObraSocial { get; set; }
    public string? NumObraSocial { get; set; }
    public string? PorcentajeIva { get; set; }
}

//Recibe el objeto del paciente a modificar
public class PacienteUpdateDto : PacienteCreateDto
{
    public long IdPaciente { get; set; }
}

// --- Profesional DTOs ---
//Recibe el objeto del profesional a crear
public class ProfesionalCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public long? Telefono { get; set; }
    public string Password { get; set; } = string.Empty;
    public TipoEspecialidad? Especialidad { get; set; }
    public string? Matricula { get; set; }
    public string? Cuit { get; set; }
    public decimal? PrecioConsulta { get; set; }
    public TipoComprobante? TipoComprobante { get; set; }
    public CondicionIVA? CondicionIva { get; set; }
}

//Recibe el objeto del profesional a modificar
public class ProfesionalUpdateDto
{
    public Guid IdProfesional { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Correo { get; set; }
    public long? Telefono { get; set; }
    public TipoEspecialidad? Especialidad { get; set; }
    public string? Matricula { get; set; }
    public string? Cuit { get; set; }
    public decimal? PrecioConsulta { get; set; }
    public TipoComprobante? TipoComprobante { get; set; }
    public CondicionIVA? CondicionIva { get; set; }
}

//Recibe el objeto del usuario administrativo a crear
public class AdministrativoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public long? Telefono { get; set; }
    public string Password { get; set; } = string.Empty;
}

// --- Auth DTOs ---
//Recibe el objeto del usuario a loguearse
public class UsuarioLoginDto
{
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

//Respuesta con los datos del usuario logueado
public class UsuarioAuthResponseDto
{
    public Guid IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string TokenJwt { get; set; } = string.Empty;
}

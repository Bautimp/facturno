using Facturno.Shared.Enums;
using System.ComponentModel.DataAnnotations;

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
    public ModalidadTurno Modalidad { get; set; } = ModalidadTurno.Presencial;
    public string? EnlaceReunion { get; set; }
}

//Recibe el objeto del turno a modificar
public class TurnoUpdateDto
{
    public long IdTurno { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public EstadoTurno Estado { get; set; }
    public string? Observacion { get; set; }
    public ModalidadTurno Modalidad { get; set; } = ModalidadTurno.Presencial;
    public string? EnlaceReunion { get; set; }
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
    public ModalidadTurno Modalidad { get; set; } = ModalidadTurno.Presencial;
    public string? EnlaceReunion { get; set; }
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
    public bool Activo { get; set; } = true;
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

// Representa el detalle de una agenda compartida con nombres descriptivos
public class AgendaCompartidaDetalleDto
{
    public string Id { get; set; } = string.Empty;
    public Guid IdProfesional { get; set; }
    public string ProfesionalNombre { get; set; } = string.Empty;
    public string IdAdministrativo { get; set; } = string.Empty;
    public string AdministrativoNombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ConfiguracionSistemaDto
{
    [Required(ErrorMessage = "La hora de inicio es requerida.")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato de hora inválido (HH:mm).")]
    public string HoraInicioTurnos { get; set; } = "07:00";

    [Required(ErrorMessage = "La hora de fin es requerida.")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato de hora inválido (HH:mm).")]
    public string HoraFinTurnos { get; set; } = "22:00";

    [Range(1, 200, ErrorMessage = "La cantidad máxima de pacientes por día debe ser entre 1 y 200.")]
    public int MaxPacientesPorDiaPorProfesional { get; set; } = 10;

    [Range(0, 100000000, ErrorMessage = "El precio máximo de consulta no puede ser negativo.")]
    public decimal PrecioMaximoConsulta { get; set; } = 100000m;

    [Range(5, 240, ErrorMessage = "El tiempo entre turnos debe ser entre 5 y 240 minutos.")]
    public int MinutosEntreTurnos { get; set; } = 45;
}

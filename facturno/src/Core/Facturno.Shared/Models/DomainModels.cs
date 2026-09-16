using Facturno.Shared.Enums;

namespace Facturno.Shared.Models;

public class Persona
{
    public long IdPersona { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? Correo { get; set; }
    public long? Telefono { get; set; }
}

public class Usuario
{
    public Guid IdUsuario { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long IdPersona { get; set; }
    public Persona? Persona { get; set; }
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;
}

public class Profesional
{
    public Guid IdProfesional { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Usuario? Usuario { get; set; }
    public TipoEspecialidad? Especialidad { get; set; }
    public string? Matricula { get; set; }
    public string? Cuit { get; set; }
    public decimal? PrecioConsulta { get; set; }
    public TipoComprobante? TipoComprobante { get; set; }
    public CondicionIVA? CondicionIva { get; set; }
}

public class ObraSocial
{
    public long IdObraSocial { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class Paciente
{
    public long IdPaciente { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Persona? Persona { get; set; }
    public string? NumDocumento { get; set; }
    public TipoDocumento? TipoDocumento { get; set; }
    public long IdObraSocial { get; set; }
    public ObraSocial? ObraSocial { get; set; }
    public string? NumObraSocial { get; set; }
    public string? PorcentajeIva { get; set; }
}

public class Turno
{
    public long IdTurno { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.Activo;
    public string? Observacion { get; set; }
    public long IdPaciente { get; set; }
    public Paciente? Paciente { get; set; }
    public Guid IdProfesional { get; set; }
    public Profesional? Profesional { get; set; }
}

public class AgendaCompartida
{
    public Guid IdAdministrativo { get; set; }
    public Guid IdProfesional { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

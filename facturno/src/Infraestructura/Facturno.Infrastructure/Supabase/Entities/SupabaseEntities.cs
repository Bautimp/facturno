using Postgrest.Attributes;
using Postgrest.Models;
using Facturno.Shared.Enums;

namespace Facturno.Infrastructure.Supabase.Entities;

[Table("personas")]
public class PersonaEntity : BaseModel
{
    [PrimaryKey("id_persona", false)]
    public long IdPersona { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("nombre")]
    public string? Nombre { get; set; }

    [Column("apellido")]
    public string? Apellido { get; set; }

    [Column("correo")]
    public string? Correo { get; set; }

    [Column("telefono")]
    public long? Telefono { get; set; }
}

[Table("usuarios")]
public class UsuarioEntity : BaseModel
{
    [PrimaryKey("id_usuario", true)]
    public string IdUsuario { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("id_persona")]
    public long IdPersona { get; set; }

    [Column("rol")]
    public string? Rol { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;
}

[Table("profesionales")]
public class ProfesionalEntity : BaseModel
{
    [PrimaryKey("id_profesional", true)]
    public string IdProfesional { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("especialidad")]
    public string? Especialidad { get; set; }

    [Column("matricula")]
    public string? Matricula { get; set; }

    [Column("cuit")]
    public string? Cuit { get; set; }

    [Column("precio_consulta")]
    public decimal? PrecioConsulta { get; set; }

    [Column("tipo_comprobante")]
    public string? TipoComprobante { get; set; }

    [Column("condicion_iva")]
    public string? CondicionIva { get; set; }
}

[Table("obras_sociales")]
public class ObraSocialEntity : BaseModel
{
    [PrimaryKey("id_obra_social", true)]
    public long IdObraSocial { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;
}

[Table("pacientes")]
public class PacienteEntity : BaseModel
{
    [PrimaryKey("id_paciente", true)]
    public long IdPaciente { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("num_documento")]
    public string? NumDocumento { get; set; }

    [Column("tipo_documento")]
    public string? TipoDocumento { get; set; }

    [Column("id_obra_social")]
    public long IdObraSocial { get; set; }

    [Column("num_obra_social")]
    public string? NumObraSocial { get; set; }

    [Column("porcentaje_iva")]
    public string? PorcentajeIva { get; set; }
}

[Table("turnos")]
public class TurnoEntity : BaseModel
{
    [PrimaryKey("id_turno", false)]
    public long IdTurno { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("id_paciente")]
    public long IdPaciente { get; set; }

    [Column("id_profesional")]
    public string IdProfesional { get; set; } = string.Empty;

    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Column("hora")]
    public TimeSpan Hora { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "Activo";

    [Column("observacion")]
    public string? Observacion { get; set; }
}

[Table("agendas_compartidas")]
public class AgendaCompartidaEntity : BaseModel
{
    [PrimaryKey("id_administrativo", true)]
    public string IdAdministrativo { get; set; } = string.Empty;

    [Column("id_profesional")]
    public string IdProfesional { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

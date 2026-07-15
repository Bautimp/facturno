using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace Facturno.Infrastructure.Supabase.Entities;

[Table("turnos")]
public class TurnoEntity : BaseModel
{
    [PrimaryKey("id_turno", false)] 
    public int IdTurno { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Column("hora")]
    public TimeSpan Hora { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("observaciones")]
    public string Observaciones { get; set; } = string.Empty;

    [Column("id_paciente")]
    public int IdPaciente { get; set; }

    [Column("id_profesional")]
    public int IdProfesional { get; set; }
}
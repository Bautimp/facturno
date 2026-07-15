using Postgrest.Attributes;
using Postgrest.Models;

namespace Facturno.Infrastructure.Supabase.Entities;

[Table("pacientes")]
public class PacienteEntity : BaseModel
{
    [PrimaryKey("id_usuario", false)] 
    public int IdUsuario { get; set; }

    [Column("num_dni_cuil")]
    public string NumDniCuil { get; set; } = string.Empty;

    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [Column("obra_social")]
    public string ObraSocial { get; set; } = string.Empty;

    [Column("num_obra_social")]
    public string NumObraSocial { get; set; } = string.Empty;

    [Column("tipo_dni_cuil")]
    public string TipoDniCuil { get; set; } = string.Empty;
}
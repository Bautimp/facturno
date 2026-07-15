using Postgrest.Attributes;
using Postgrest.Models;

namespace Facturno.Infrastructure.Supabase.Entities;

[Table("profesionales")]
public class ProfesionalEntity : BaseModel
{
    [PrimaryKey("id_usuario", false)]
    public int IdUsuario { get; set; }

    [Column("matricula")]
    public string Matricula { get; set; } = string.Empty;

    [Column("especialidad")]
    public string Especialidad { get; set; } = string.Empty;
}
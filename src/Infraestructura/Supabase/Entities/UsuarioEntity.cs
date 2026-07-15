using Postgrest.Attributes;
using Postgrest.Models;

namespace Facturno.Infrastructure.Supabase.Entities;

[Table("usuarios")]
public class UsuarioEntity : BaseModel
{
    [PrimaryKey("id_usuario", false)]
    public int IdUsuario { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("contrasena")]
    public string Contrasena { get; set; } = string.Empty;

    [Column("rol")] // Debe coincidir con la mayúscula de tu SQL
    public string rol { get; set; } = string.Empty;
}
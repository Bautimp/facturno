using Facturno.Shared.Enums; // Asegúrate de tener el enum RolUsuario creado

namespace Facturno.Shared.Models;

public abstract class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;

    // Propiedad agregada para RBAC
    public RolUsuario Rol { get; set; } 
}
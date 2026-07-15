using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public UsuarioRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        // En C# no podemos instanciar la clase abstracta Usuario directamente.
        // Así que usamos una clase anónima o mapeamos según el rol si fuera necesario.
        // Por ahora, devolveremos una representación concreta básica o un DTO de auth en el futuro.
        
        var response = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.Email == email)
            .Single();

        if (response == null) return null;

        // Aquí aplicamos el polimorfismo instanciando la clase concreta según el Rol
        var rol = Enum.Parse<RolUsuario>(response.Rol);
        
        Usuario usuario = rol switch
        {
            RolUsuario.Paciente => new Paciente(),
            RolUsuario.Profesional => new Profesional(),
            _ => new UsuarioBaseTemporal() // Para la Secretaria, que hereda directamente sin tabla extra
        };

        usuario.IdUsuario = response.IdUsuario;
        usuario.Nombre = response.Nombre;
        usuario.Apellido = response.Apellido;
        usuario.Email = response.Email;
        usuario.Contrasena = response.Contrasena;
        usuario.Rol = rol;

        return usuario;
    }
    
    // Clase auxiliar privada solo para instanciar roles que no tienen tabla extra (ej. Secretaria, Admin)
    private class UsuarioBaseTemporal : Usuario { }
}
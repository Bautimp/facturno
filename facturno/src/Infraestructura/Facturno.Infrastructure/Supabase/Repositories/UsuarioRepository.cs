using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public UsuarioRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(Guid idUsuario)
    {
        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdUsuario == idUsuario.ToString())
            .Single();

        if (userRes == null) return null;

        var personaRes = await _supabaseClient.From<PersonaEntity>()
            .Where(x => x.IdPersona == userRes.IdPersona)
            .Single();

        return MapearAUsuario(userRes, personaRes);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var personaRes = await _supabaseClient.From<PersonaEntity>()
            .Where(x => x.Correo == correo)
            .Single();

        if (personaRes == null) return null;

        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdPersona == personaRes.IdPersona)
            .Single();

        if (userRes == null) return null;

        return MapearAUsuario(userRes, personaRes);
    }

    public async Task<bool> CambiarEstadoActivoAsync(Guid idUsuario, bool activo)
    {
        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdUsuario == idUsuario.ToString())
            .Single();

        if (userRes == null) return false;

        userRes.Activo = activo;
        await _supabaseClient.From<UsuarioEntity>().Update(userRes);
        return true;
    }

    private static Usuario MapearAUsuario(UsuarioEntity entity, PersonaEntity? personaEntity)
    {
        return new Usuario
        {
            IdUsuario = Guid.TryParse(entity.IdUsuario, out var guid) ? guid : Guid.Empty,
            CreatedAt = entity.CreatedAt,
            IdPersona = entity.IdPersona,
            Rol = Enum.TryParse<RolUsuario>(entity.Rol, out var rol) ? rol : RolUsuario.Profesional,
            Activo = entity.Activo,
            Persona = personaEntity == null ? null : new Persona
            {
                IdPersona = personaEntity.IdPersona,
                Nombre = personaEntity.Nombre,
                Apellido = personaEntity.Apellido,
                Correo = personaEntity.Correo,
                Telefono = personaEntity.Telefono
            }
        };
    }
}

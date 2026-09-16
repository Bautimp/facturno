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
        var userIdStr = idUsuario.ToString();
        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdUsuario == userIdStr)
            .Get();

        var userEntity = userRes.Models.FirstOrDefault();
        if (userEntity == null) return null;

        var personaRes = await _supabaseClient.From<PersonaEntity>()
            .Where(x => x.IdPersona == userEntity.IdPersona)
            .Get();

        var personaEntity = personaRes.Models.FirstOrDefault();

        return MapearAUsuario(userEntity, personaEntity);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        var personaRes = await _supabaseClient.From<PersonaEntity>()
            .Where(x => x.Correo == correo)
            .Get();

        var personaEntity = personaRes.Models.FirstOrDefault();
        if (personaEntity == null) return null;

        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdPersona == personaEntity.IdPersona)
            .Get();

        var userEntity = userRes.Models.FirstOrDefault();
        if (userEntity == null) return null;

        return MapearAUsuario(userEntity, personaEntity);
    }

    public async Task<Usuario> CrearConPersonaAsync(Usuario usuario, Persona persona)
    {
        var personaEntity = new PersonaEntity
        {
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            Correo = persona.Correo,
            Telefono = persona.Telefono
        };

        var personaRes = await _supabaseClient.From<PersonaEntity>().Insert(personaEntity);
        var personaCreada = personaRes.Models.First();

        var usuarioEntity = new UsuarioEntity
        {
            IdUsuario = usuario.IdUsuario.ToString(),
            IdPersona = personaCreada.IdPersona,
            Rol = usuario.Rol.ToString(),
            Activo = usuario.Activo
        };

        await _supabaseClient.From<UsuarioEntity>().Insert(usuarioEntity);

        usuario.IdPersona = personaCreada.IdPersona;
        usuario.Persona = new Persona
        {
            IdPersona = personaCreada.IdPersona,
            Nombre = personaCreada.Nombre,
            Apellido = personaCreada.Apellido,
            Correo = personaCreada.Correo,
            Telefono = personaCreada.Telefono
        };

        return usuario;
    }

    public async Task<bool> CambiarEstadoActivoAsync(Guid idUsuario, bool activo)
    {
        var userIdStr = idUsuario.ToString();
        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdUsuario == userIdStr)
            .Get();

        var userEntity = userRes.Models.FirstOrDefault();
        if (userEntity == null) return false;

        userEntity.Activo = activo;
        await _supabaseClient.From<UsuarioEntity>().Update(userEntity);
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

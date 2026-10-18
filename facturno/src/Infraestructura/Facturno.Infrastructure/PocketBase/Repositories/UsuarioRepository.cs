using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;

namespace Facturno.Infrastructure.PocketBase.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly PocketBaseClient _client;

    public UsuarioRepository(PocketBaseClient client)
    {
        _client = client;
    }

    public async Task<Usuario?> ObtenerPorId(Guid idUsuario)
    {
        var idStr = idUsuario.ToString();
        var userRecord = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_usuario='{idStr}'");
        if (userRecord == null) return null;

        PersonaPbRecord? personaRecord = null;
        if (userRecord.IdPersona > 0)
        {
            personaRecord = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={userRecord.IdPersona}");
        }

        return MapearAUsuario(userRecord, personaRecord);
    }

    public async Task<Usuario?> ObtenerPorCorreo(string correo)
    {
        // 1. Buscar en users por email
        var userRecord = await _client.GetFirst<PocketBaseUserRecord>("users", $"email='{correo}'");
        PersonaPbRecord? personaRecord = null;

        if (userRecord != null)
        {
            if (userRecord.IdPersona > 0)
            {
                personaRecord = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={userRecord.IdPersona}");
            }
            return MapearAUsuario(userRecord, personaRecord);
        }

        // 2. Fallback: buscar en personas por correo
        personaRecord = await _client.GetFirst<PersonaPbRecord>("personas", $"correo='{correo}'");
        if (personaRecord != null)
        {
            userRecord = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_persona={personaRecord.IdPersona}");
            if (userRecord != null)
            {
                return MapearAUsuario(userRecord, personaRecord);
            }
        }

        return null;
    }

    public async Task<Usuario> CrearConPersona(Usuario usuario, Persona persona)
    {
        // 1. Guardar Persona
        if (persona.IdPersona <= 0)
        {
            var todasPersonas = await _client.GetList<PersonaPbRecord>("personas");
            persona.IdPersona = todasPersonas.Any() ? todasPersonas.Max(p => p.IdPersona) + 1 : 1;

            var personaPayload = new
            {
                id_persona = persona.IdPersona,
                nombre = persona.Nombre ?? string.Empty,
                apellido = persona.Apellido ?? string.Empty,
                correo = persona.Correo,
                telefono = persona.Telefono
            };
            await _client.Create<PersonaPbRecord>("personas", personaPayload);
        }

        // 2. Guardar Usuario
        if (usuario.IdUsuario == Guid.Empty)
        {
            usuario.IdUsuario = Guid.NewGuid();
        }

        var email = !string.IsNullOrWhiteSpace(persona.Correo) ? persona.Correo : $"{usuario.IdUsuario}@facturno.local";
        var username = email.Split('@')[0].Replace(".", "_");

        var existing = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_usuario='{usuario.IdUsuario}'");
        if (existing == null)
        {
            existing = await _client.CreateUser(
                email: email,
                password: "Facturno123!",
                username: username,
                rol: usuario.Rol.ToString(),
                idPersona: persona.IdPersona,
                idUsuario: usuario.IdUsuario.ToString()
            );
        }

        usuario.IdPersona = persona.IdPersona;
        usuario.Persona = persona;
        usuario.CreatedAt = existing.Created;
        return usuario;
    }

    public async Task<bool> CambiarEstadoActivo(Guid idUsuario, bool activo)
    {
        var idStr = idUsuario.ToString();
        var userRecord = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_usuario='{idStr}'");
        if (userRecord == null) return false;

        await _client.Update<PocketBaseUserRecord>("users", userRecord.Id, new { activo = activo });
        return true;
    }

    private static Usuario MapearAUsuario(PocketBaseUserRecord record, PersonaPbRecord? persona)
    {
        return new Usuario
        {
            IdUsuario = Guid.TryParse(record.IdUsuario, out var guid) ? guid : Guid.Empty,
            CreatedAt = record.Created,
            IdPersona = record.IdPersona,
            Rol = Enum.TryParse<RolUsuario>(record.Rol, out var rol) ? rol : RolUsuario.Profesional,
            Activo = record.Activo,
            Persona = persona == null ? null : new Persona
            {
                IdPersona = persona.IdPersona,
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Correo = persona.Correo,
                Telefono = persona.Telefono
            }
        };
    }
}

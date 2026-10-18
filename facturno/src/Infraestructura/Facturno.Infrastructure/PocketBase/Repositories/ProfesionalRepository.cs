using System.Text.Json.Serialization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;

namespace Facturno.Infrastructure.PocketBase.Repositories;

internal class ProfesionalPbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_profesional")]
    public string IdProfesional { get; set; } = string.Empty;

    [JsonPropertyName("especialidad")]
    public string? Especialidad { get; set; }

    [JsonPropertyName("matricula")]
    public string? Matricula { get; set; }

    [JsonPropertyName("cuit")]
    public string? Cuit { get; set; }

    [JsonPropertyName("precio_consulta")]
    public decimal? PrecioConsulta { get; set; }

    [JsonPropertyName("tipo_comprobante")]
    public string? TipoComprobante { get; set; }

    [JsonPropertyName("condicion_iva")]
    public string? CondicionIva { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly PocketBaseClient _client;
    private const string CollectionName = "profesionales";

    public ProfesionalRepository(PocketBaseClient client)
    {
        _client = client;
    }

    public async Task<Profesional?> ObtenerPorId(Guid idProfesional)
    {
        var profIdStr = idProfesional.ToString();
        var profRecord = await _client.GetFirst<ProfesionalPbRecord>(CollectionName, $"id_profesional='{profIdStr}'");
        if (profRecord == null) return null;

        var userRecord = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_usuario='{profIdStr}'");
        PersonaPbRecord? personaRecord = null;
        if (userRecord != null && userRecord.IdPersona > 0)
        {
            personaRecord = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={userRecord.IdPersona}");
        }

        return MapearAProfesional(profRecord, userRecord, personaRecord);
    }

    public async Task<List<Profesional>> ObtenerTodos()
    {
        var profs = await _client.GetList<ProfesionalPbRecord>(CollectionName);
        var users = await _client.GetList<PocketBaseUserRecord>("users");
        var personas = await _client.GetList<PersonaPbRecord>("personas");

        var usersDict = users
            .Where(u => !string.IsNullOrEmpty(u.IdUsuario))
            .DistinctBy(u => u.IdUsuario)
            .ToDictionary(u => u.IdUsuario);

        var personasDict = personas
            .DistinctBy(p => p.IdPersona)
            .ToDictionary(p => p.IdPersona);

        return profs.Select(p =>
        {
            usersDict.TryGetValue(p.IdProfesional, out var user);
            PersonaPbRecord? persona = null;
            if (user != null && personasDict.TryGetValue(user.IdPersona, out var per))
            {
                persona = per;
            }
            return MapearAProfesional(p, user, persona);
        }).ToList();
    }

    public async Task<Profesional> Crear(Profesional profesional, Usuario usuario, Persona persona)
    {
        // 1. Guardar Persona si no existe
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

        // 2. Resolver Usuario
        if (usuario.IdUsuario == Guid.Empty)
        {
            usuario.IdUsuario = Guid.NewGuid();
        }
        profesional.IdProfesional = usuario.IdUsuario;

        var existingUser = await _client.GetFirst<PocketBaseUserRecord>("users", $"id_usuario='{usuario.IdUsuario}'");
        if (existingUser == null)
        {
            var email = !string.IsNullOrWhiteSpace(persona.Correo) ? persona.Correo : $"{usuario.IdUsuario}@facturno.local";
            var username = email.Split('@')[0].Replace(".", "_");
            existingUser = await _client.CreateUser(
                email: email,
                password: "Facturno123!",
                username: username,
                rol: usuario.Rol.ToString(),
                idPersona: persona.IdPersona,
                idUsuario: usuario.IdUsuario.ToString()
            );
        }

        // 3. Crear Profesional
        var profPayload = new
        {
            id_profesional = profesional.IdProfesional.ToString(),
            especialidad = profesional.Especialidad?.ToString(),
            matricula = profesional.Matricula,
            cuit = profesional.Cuit,
            precio_consulta = profesional.PrecioConsulta,
            tipo_comprobante = profesional.TipoComprobante?.ToString(),
            condicion_iva = profesional.CondicionIva?.ToString()
        };

        var createdProf = await _client.Create<ProfesionalPbRecord>(CollectionName, profPayload);

        var personaRecord = new PersonaPbRecord
        {
            IdPersona = persona.IdPersona,
            Nombre = persona.Nombre ?? string.Empty,
            Apellido = persona.Apellido ?? string.Empty,
            Correo = persona.Correo,
            Telefono = persona.Telefono
        };

        return MapearAProfesional(createdProf, existingUser, personaRecord);
    }

    public async Task<Profesional> Actualizar(Profesional profesional, Persona persona)
    {
        // 1. Actualizar Persona
        if (persona.IdPersona > 0)
        {
            var existingPersona = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={persona.IdPersona}");
            if (existingPersona != null)
            {
                var personaPayload = new
                {
                    nombre = persona.Nombre,
                    apellido = persona.Apellido,
                    correo = persona.Correo,
                    telefono = persona.Telefono
                };
                await _client.Update<PersonaPbRecord>("personas", existingPersona.Id, personaPayload);
            }
        }

        // 2. Actualizar Profesional
        var existingProf = await _client.GetFirst<ProfesionalPbRecord>(CollectionName, $"id_profesional='{profesional.IdProfesional}'");
        if (existingProf != null)
        {
            var profPayload = new
            {
                especialidad = profesional.Especialidad?.ToString(),
                matricula = profesional.Matricula,
                cuit = profesional.Cuit,
                precio_consulta = profesional.PrecioConsulta,
                tipo_comprobante = profesional.TipoComprobante?.ToString(),
                condicion_iva = profesional.CondicionIva?.ToString()
            };
            existingProf = await _client.Update<ProfesionalPbRecord>(CollectionName, existingProf.Id, profPayload);
        }
        else
        {
            var profPayload = new
            {
                id_profesional = profesional.IdProfesional.ToString(),
                especialidad = profesional.Especialidad?.ToString(),
                matricula = profesional.Matricula,
                cuit = profesional.Cuit,
                precio_consulta = profesional.PrecioConsulta,
                tipo_comprobante = profesional.TipoComprobante?.ToString(),
                condicion_iva = profesional.CondicionIva?.ToString()
            };
            existingProf = await _client.Create<ProfesionalPbRecord>(CollectionName, profPayload);
        }

        return await ObtenerPorId(profesional.IdProfesional) 
            ?? MapearAProfesional(existingProf, null, null);
    }

    private static Profesional MapearAProfesional(ProfesionalPbRecord prof, PocketBaseUserRecord? user, PersonaPbRecord? persona)
    {
        return new Profesional
        {
            IdProfesional = Guid.TryParse(prof.IdProfesional, out var guid) ? guid : Guid.Empty,
            CreatedAt = prof.Created,
            Especialidad = Enum.TryParse<TipoEspecialidad>(prof.Especialidad, out var esp) ? esp : null,
            Matricula = prof.Matricula,
            Cuit = prof.Cuit,
            PrecioConsulta = prof.PrecioConsulta,
            TipoComprobante = Enum.TryParse<TipoComprobante>(prof.TipoComprobante, out var tc) ? tc : null,
            CondicionIva = Enum.TryParse<CondicionIVA>(prof.CondicionIva, out var ci) ? ci : null,
            Usuario = user == null ? null : new Usuario
            {
                IdUsuario = Guid.TryParse(user.IdUsuario, out var uGuid) ? uGuid : Guid.Empty,
                IdPersona = user.IdPersona,
                Rol = Enum.TryParse<RolUsuario>(user.Rol, out var rol) ? rol : RolUsuario.Profesional,
                Activo = user.Activo,
                Persona = persona == null ? null : new Persona
                {
                    IdPersona = persona.IdPersona,
                    Nombre = persona.Nombre,
                    Apellido = persona.Apellido,
                    Correo = persona.Correo,
                    Telefono = persona.Telefono
                }
            }
        };
    }
}

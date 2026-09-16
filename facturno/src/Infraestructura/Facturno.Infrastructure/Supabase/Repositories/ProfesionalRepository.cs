using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase.Repositories;

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public ProfesionalRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Profesional?> ObtenerPorIdAsync(Guid idProfesional)
    {
        var profIdStr = idProfesional.ToString();
        var profRes = await _supabaseClient.From<ProfesionalEntity>()
            .Where(x => x.IdProfesional == profIdStr)
            .Single();

        if (profRes == null) return null;

        var userRes = await _supabaseClient.From<UsuarioEntity>()
            .Where(x => x.IdUsuario == profIdStr)
            .Single();

        PersonaEntity? personaRes = null;
        if (userRes != null)
        {
            personaRes = await _supabaseClient.From<PersonaEntity>()
                .Where(x => x.IdPersona == userRes.IdPersona)
                .Single();
        }

        return MapearAProfesional(profRes, userRes, personaRes);
    }

    public async Task<List<Profesional>> ObtenerTodosAsync()
    {
        var profsRes = await _supabaseClient.From<ProfesionalEntity>().Get();
        var usersRes = await _supabaseClient.From<UsuarioEntity>().Get();
        var personasRes = await _supabaseClient.From<PersonaEntity>().Get();

        var usersDict = usersRes.Models.ToDictionary(u => u.IdUsuario);
        var personasDict = personasRes.Models.ToDictionary(p => p.IdPersona);

        return profsRes.Models.Select(p =>
        {
            var user = usersDict.GetValueOrDefault(p.IdProfesional);
            var persona = user != null ? personasDict.GetValueOrDefault(user.IdPersona) : null;
            return MapearAProfesional(p, user, persona);
        }).ToList();
    }

    public async Task<Profesional> CrearAsync(Profesional profesional, Usuario usuario, Persona persona)
    {
        var rpcParams = new Dictionary<string, object>
        {
            { "p_nombre", persona.Nombre ?? string.Empty },
            { "p_apellido", persona.Apellido ?? string.Empty },
            { "p_correo", persona.Correo ?? string.Empty },
            { "p_telefono", persona.Telefono ?? 0 },
            { "p_especialidad", profesional.Especialidad?.ToString() ?? string.Empty },
            { "p_matricula", profesional.Matricula ?? string.Empty },
            { "p_cuit", profesional.Cuit ?? string.Empty },
            { "p_precio_consulta", profesional.PrecioConsulta ?? 0 },
            { "p_tipo_comprobante", profesional.TipoComprobante?.ToString() ?? string.Empty },
            { "p_condicion_iva", profesional.CondicionIva?.ToString() ?? string.Empty }
        };

        var response = await _supabaseClient.Postgrest.Rpc("crear_profesional", rpcParams);
        if (response != null && !string.IsNullOrWhiteSpace(response.Content))
        {
            var createdIdString = response.Content.Trim('"');
            if (Guid.TryParse(createdIdString, out var createdGuid))
            {
                var result = await ObtenerPorIdAsync(createdGuid);
                if (result != null) return result;
            }
        }

        throw new Exception("No se pudo obtener el ID del profesional creado desde la función RPC 'crear_profesional'.");
    }

    public async Task<Profesional> ActualizarAsync(Profesional profesional, Persona persona)
    {
        var personaEntity = new PersonaEntity
        {
            IdPersona = persona.IdPersona,
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            Correo = persona.Correo,
            Telefono = persona.Telefono
        };
        await _supabaseClient.From<PersonaEntity>().Update(personaEntity);

        var profesionalEntity = new ProfesionalEntity
        {
            IdProfesional = profesional.IdProfesional.ToString(),
            Especialidad = profesional.Especialidad?.ToString(),
            Matricula = profesional.Matricula,
            Cuit = profesional.Cuit,
            PrecioConsulta = profesional.PrecioConsulta,
            TipoComprobante = profesional.TipoComprobante?.ToString(),
            CondicionIva = profesional.CondicionIva?.ToString()
        };
        await _supabaseClient.From<ProfesionalEntity>().Update(profesionalEntity);

        return MapearAProfesional(profesionalEntity, null, personaEntity);
    }

    private static Profesional MapearAProfesional(ProfesionalEntity entity, UsuarioEntity? userEntity, PersonaEntity? personaEntity)
    {
        return new Profesional
        {
            IdProfesional = Guid.TryParse(entity.IdProfesional, out var guid) ? guid : Guid.Empty,
            CreatedAt = entity.CreatedAt,
            Especialidad = Enum.TryParse<TipoEspecialidad>(entity.Especialidad, out var esp) ? esp : null,
            Matricula = entity.Matricula,
            Cuit = entity.Cuit,
            PrecioConsulta = entity.PrecioConsulta,
            TipoComprobante = Enum.TryParse<TipoComprobante>(entity.TipoComprobante, out var tc) ? tc : null,
            CondicionIva = Enum.TryParse<CondicionIVA>(entity.CondicionIva, out var ci) ? ci : null,
            Usuario = userEntity == null ? null : new Usuario
            {
                IdUsuario = Guid.TryParse(userEntity.IdUsuario, out var uGuid) ? uGuid : Guid.Empty,
                IdPersona = userEntity.IdPersona,
                Rol = Enum.TryParse<RolUsuario>(userEntity.Rol, out var rol) ? rol : RolUsuario.Profesional,
                Activo = userEntity.Activo,
                Persona = personaEntity == null ? null : new Persona
                {
                    IdPersona = personaEntity.IdPersona,
                    Nombre = personaEntity.Nombre,
                    Apellido = personaEntity.Apellido,
                    Correo = personaEntity.Correo,
                    Telefono = personaEntity.Telefono
                }
            }
        };
    }
}

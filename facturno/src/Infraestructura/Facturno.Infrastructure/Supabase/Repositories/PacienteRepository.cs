using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public PacienteRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Paciente?> ObtenerPorIdAsync(long idPaciente)
    {
        var pacienteRes = await _supabaseClient.From<PacienteEntity>()
            .Where(x => x.IdPaciente == idPaciente)
            .Single();

        if (pacienteRes == null) return null;

        var personaRes = await _supabaseClient.From<PersonaEntity>()
            .Where(x => x.IdPersona == idPaciente)
            .Single();

        return MapearAPaciente(pacienteRes, personaRes);
    }

    public async Task<List<Paciente>> ObtenerTodosAsync()
    {
        var pacientesRes = await _supabaseClient.From<PacienteEntity>().Get();
        var personasRes = await _supabaseClient.From<PersonaEntity>().Get();

        var personasDict = personasRes.Models.ToDictionary(p => p.IdPersona);

        return pacientesRes.Models.Select(p =>
            MapearAPaciente(p, personasDict.GetValueOrDefault(p.IdPaciente))
        ).ToList();
    }

    public async Task<Paciente> CrearAsync(Paciente paciente, Persona persona)
    {
        var personaEntity = new PersonaEntity
        {
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            Correo = persona.Correo,
            Telefono = persona.Telefono
        };

        var personaCreated = (await _supabaseClient.From<PersonaEntity>().Insert(personaEntity)).Models.First();

        var pacienteEntity = new PacienteEntity
        {
            IdPaciente = personaCreated.IdPersona,
            NumDocumento = paciente.NumDocumento,
            TipoDocumento = paciente.TipoDocumento?.ToString(),
            IdObraSocial = paciente.IdObraSocial,
            NumObraSocial = paciente.NumObraSocial,
            PorcentajeIva = paciente.PorcentajeIva
        };

        var pacienteCreated = (await _supabaseClient.From<PacienteEntity>().Insert(pacienteEntity)).Models.First();
        return MapearAPaciente(pacienteCreated, personaCreated);
    }

    public async Task<Paciente> ActualizarAsync(Paciente paciente, Persona persona)
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

        var pacienteEntity = new PacienteEntity
        {
            IdPaciente = paciente.IdPaciente,
            NumDocumento = paciente.NumDocumento,
            TipoDocumento = paciente.TipoDocumento?.ToString(),
            IdObraSocial = paciente.IdObraSocial,
            NumObraSocial = paciente.NumObraSocial,
            PorcentajeIva = paciente.PorcentajeIva
        };
        await _supabaseClient.From<PacienteEntity>().Update(pacienteEntity);

        return MapearAPaciente(pacienteEntity, personaEntity);
    }

    private static Paciente MapearAPaciente(PacienteEntity entity, PersonaEntity? personaEntity)
    {
        return new Paciente
        {
            IdPaciente = entity.IdPaciente,
            CreatedAt = entity.CreatedAt,
            NumDocumento = entity.NumDocumento,
            TipoDocumento = Enum.TryParse<TipoDocumento>(entity.TipoDocumento, out var td) ? td : null,
            IdObraSocial = entity.IdObraSocial,
            NumObraSocial = entity.NumObraSocial,
            PorcentajeIva = entity.PorcentajeIva,
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

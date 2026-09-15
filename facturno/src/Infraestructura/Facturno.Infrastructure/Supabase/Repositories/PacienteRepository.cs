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
        var idOs = paciente.IdObraSocial > 0 ? paciente.IdObraSocial : 1;
        await AsegurarObraSocialExisteAsync(idOs);
        paciente.IdObraSocial = idOs;

        var correoNormalizado = string.IsNullOrWhiteSpace(persona.Correo) ? null : persona.Correo.Trim();
        var numObraSocialNormalizado = (paciente.IdObraSocial == 1 || string.IsNullOrWhiteSpace(paciente.NumObraSocial)) 
            ? null 
            : paciente.NumObraSocial.Trim();

        PersonaEntity personaCreated;

        if (!string.IsNullOrEmpty(correoNormalizado))
        {
            var existingPersonaRes = await _supabaseClient.From<PersonaEntity>()
                .Where(x => x.Correo == correoNormalizado)
                .Get();

            var existingPersona = existingPersonaRes.Models.FirstOrDefault();
            if (existingPersona != null)
            {
                personaCreated = existingPersona;
                // Actualizar datos de la persona si cambiaron
                personaCreated.Nombre = persona.Nombre;
                personaCreated.Apellido = persona.Apellido;
                personaCreated.Telefono = persona.Telefono;
                await _supabaseClient.From<PersonaEntity>().Update(personaCreated);
            }
            else
            {
                var personaEntity = new PersonaEntity
                {
                    Nombre = persona.Nombre,
                    Apellido = persona.Apellido,
                    Correo = correoNormalizado,
                    Telefono = persona.Telefono
                };
                personaCreated = (await _supabaseClient.From<PersonaEntity>().Insert(personaEntity)).Models.First();
            }
        }
        else
        {
            var personaEntity = new PersonaEntity
            {
                Nombre = persona.Nombre,
                Apellido = persona.Apellido,
                Correo = null,
                Telefono = persona.Telefono
            };
            personaCreated = (await _supabaseClient.From<PersonaEntity>().Insert(personaEntity)).Models.First();
        }

        // Verificar si ya existe como paciente
        var existingPacRes = await _supabaseClient.From<PacienteEntity>()
            .Where(x => x.IdPaciente == personaCreated.IdPersona)
            .Get();

        var existingPac = existingPacRes.Models.FirstOrDefault();
        PacienteEntity pacienteCreated;

        if (existingPac != null)
        {
            existingPac.NumDocumento = paciente.NumDocumento;
            existingPac.TipoDocumento = paciente.TipoDocumento?.ToString();
            existingPac.IdObraSocial = paciente.IdObraSocial;
            existingPac.NumObraSocial = numObraSocialNormalizado;
            existingPac.PorcentajeIva = paciente.PorcentajeIva;

            await _supabaseClient.From<PacienteEntity>().Update(existingPac);
            pacienteCreated = existingPac;
        }
        else
        {
            var pacienteEntity = new PacienteEntity
            {
                IdPaciente = personaCreated.IdPersona,
                NumDocumento = paciente.NumDocumento,
                TipoDocumento = paciente.TipoDocumento?.ToString(),
                IdObraSocial = paciente.IdObraSocial,
                NumObraSocial = numObraSocialNormalizado,
                PorcentajeIva = paciente.PorcentajeIva
            };
            pacienteCreated = (await _supabaseClient.From<PacienteEntity>().Insert(pacienteEntity)).Models.First();
        }

        return MapearAPaciente(pacienteCreated, personaCreated);
    }

    public async Task<Paciente> ActualizarAsync(Paciente paciente, Persona persona)
    {
        var correoNormalizado = string.IsNullOrWhiteSpace(persona.Correo) ? null : persona.Correo.Trim();
        var numObraSocialNormalizado = (paciente.IdObraSocial == 1 || string.IsNullOrWhiteSpace(paciente.NumObraSocial)) 
            ? null 
            : paciente.NumObraSocial.Trim();

        var personaEntity = new PersonaEntity
        {
            IdPersona = persona.IdPersona,
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            Correo = correoNormalizado,
            Telefono = persona.Telefono
        };
        await _supabaseClient.From<PersonaEntity>().Update(personaEntity);

        var pacienteEntity = new PacienteEntity
        {
            IdPaciente = paciente.IdPaciente,
            NumDocumento = paciente.NumDocumento,
            TipoDocumento = paciente.TipoDocumento?.ToString(),
            IdObraSocial = paciente.IdObraSocial,
            NumObraSocial = numObraSocialNormalizado,
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

    private async Task AsegurarObraSocialExisteAsync(long idObraSocial)
    {
        if (idObraSocial <= 0) return;
        try
        {
            var res = await _supabaseClient.From<ObraSocialEntity>()
                .Where(x => x.IdObraSocial == idObraSocial)
                .Get();

            if (!res.Models.Any())
            {
                var os = new ObraSocialEntity
                {
                    IdObraSocial = idObraSocial,
                    Nombre = idObraSocial == 1 ? "Particular" : $"Obra Social #{idObraSocial}",
                    Activo = true
                };
                await _supabaseClient.From<ObraSocialEntity>().Insert(os);
            }
        }
        catch
        {
            // Ignorar si ya existe o falla concurrencia
        }
    }
}

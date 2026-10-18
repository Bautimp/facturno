using System.Text.Json.Serialization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;

namespace Facturno.Infrastructure.PocketBase.Repositories;

internal class PacientePbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_paciente")]
    public long IdPaciente { get; set; }

    [JsonPropertyName("num_documento")]
    public string? NumDocumento { get; set; }

    [JsonPropertyName("tipo_documento")]
    public string? TipoDocumento { get; set; }

    [JsonPropertyName("id_obra_social")]
    public long IdObraSocial { get; set; }

    [JsonPropertyName("num_obra_social")]
    public string? NumObraSocial { get; set; }

    [JsonPropertyName("porcentaje_iva")]
    public string? PorcentajeIva { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class PacienteRepository : IPacienteRepository
{
    private readonly PocketBaseClient _client;

    public PacienteRepository(PocketBaseClient client)
    {
        _client = client;
    }

    public async Task<Paciente?> ObtenerPorId(long idPaciente)
    {
        var pacRecord = await _client.GetFirst<PacientePbRecord>("pacientes", $"id_paciente={idPaciente}");
        if (pacRecord == null) return null;

        var perRecord = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={idPaciente}");
        return MapearAPaciente(pacRecord, perRecord);
    }

    public async Task<List<Paciente>> ObtenerTodos()
    {
        var pacRecords = await _client.GetList<PacientePbRecord>("pacientes", sort: "id_paciente");
        var perRecords = await _client.GetList<PersonaPbRecord>("personas");

        var perDict = perRecords.ToDictionary(p => p.IdPersona);

        return pacRecords.Select(p =>
            MapearAPaciente(p, perDict.GetValueOrDefault(p.IdPaciente))
        ).ToList();
    }

    public async Task<Paciente> Crear(Paciente paciente, Persona persona)
    {
        // 1. Resolver id_persona
        var allPersonas = await _client.GetList<PersonaPbRecord>("personas");
        long newIdPersona = allPersonas.Any() ? allPersonas.Max(p => p.IdPersona) + 1 : 1;

        var personaPayload = new
        {
            id_persona = newIdPersona,
            nombre = persona.Nombre,
            apellido = persona.Apellido,
            correo = persona.Correo,
            telefono = persona.Telefono
        };

        var personaCreated = await _client.Create<PersonaPbRecord>("personas", personaPayload);

        // 2. Crear paciente
        var pacientePayload = new
        {
            id_paciente = newIdPersona,
            num_documento = paciente.NumDocumento,
            tipo_documento = paciente.TipoDocumento?.ToString(),
            id_obra_social = paciente.IdObraSocial > 0 ? paciente.IdObraSocial : 1,
            num_obra_social = paciente.IdObraSocial == 1 ? null : paciente.NumObraSocial,
            porcentaje_iva = paciente.PorcentajeIva
        };

        var pacienteCreated = await _client.Create<PacientePbRecord>("pacientes", pacientePayload);
        return MapearAPaciente(pacienteCreated, personaCreated);
    }

    public async Task<Paciente> Actualizar(Paciente paciente, Persona persona)
    {
        // Actualizar Persona
        var existingPersona = await _client.GetFirst<PersonaPbRecord>("personas", $"id_persona={paciente.IdPaciente}");
        if (existingPersona != null)
        {
            var personaPayload = new
            {
                nombre = persona.Nombre,
                apellido = persona.Apellido,
                correo = persona.Correo,
                telefono = persona.Telefono
            };
            existingPersona = await _client.Update<PersonaPbRecord>("personas", existingPersona.Id, personaPayload);
        }

        // Actualizar Paciente
        var existingPaciente = await _client.GetFirst<PacientePbRecord>("pacientes", $"id_paciente={paciente.IdPaciente}");
        if (existingPaciente != null)
        {
            var pacientePayload = new
            {
                num_documento = paciente.NumDocumento,
                tipo_documento = paciente.TipoDocumento?.ToString(),
                id_obra_social = paciente.IdObraSocial > 0 ? paciente.IdObraSocial : 1,
                num_obra_social = paciente.IdObraSocial == 1 ? null : paciente.NumObraSocial,
                porcentaje_iva = paciente.PorcentajeIva
            };
            existingPaciente = await _client.Update<PacientePbRecord>("pacientes", existingPaciente.Id, pacientePayload);
        }

        return MapearAPaciente(existingPaciente ?? new PacientePbRecord { IdPaciente = paciente.IdPaciente }, existingPersona);
    }

    private static Paciente MapearAPaciente(PacientePbRecord pac, PersonaPbRecord? per)
    {
        return new Paciente
        {
            IdPaciente = pac.IdPaciente,
            CreatedAt = pac.Created,
            NumDocumento = pac.NumDocumento,
            TipoDocumento = Enum.TryParse<TipoDocumento>(pac.TipoDocumento, out var td) ? td : null,
            IdObraSocial = pac.IdObraSocial,
            NumObraSocial = pac.NumObraSocial,
            PorcentajeIva = pac.PorcentajeIva,
            Persona = per == null ? null : new Persona
            {
                IdPersona = per.IdPersona,
                Nombre = per.Nombre,
                Apellido = per.Apellido,
                Correo = per.Correo,
                Telefono = per.Telefono
            }
        };
    }
}

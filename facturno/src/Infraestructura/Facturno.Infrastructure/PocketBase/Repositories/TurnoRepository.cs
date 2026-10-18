using System.Text.Json.Serialization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;

namespace Facturno.Infrastructure.PocketBase.Repositories;

internal class TurnoPbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_turno")]
    public long IdTurno { get; set; }

    [JsonPropertyName("id_paciente")]
    public long IdPaciente { get; set; }

    [JsonPropertyName("id_profesional")]
    public string IdProfesional { get; set; } = string.Empty;

    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [JsonPropertyName("hora")]
    public string Hora { get; set; } = string.Empty;

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("observacion")]
    public string? Observacion { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class TurnoRepository : ITurnoRepository
{
    private readonly PocketBaseClient _client;
    private const string CollectionName = "turnos";

    public TurnoRepository(PocketBaseClient client)
    {
        _client = client;
    }

    public async Task<Turno?> ObtenerPorId(long idTurno)
    {
        var record = await _client.GetFirst<TurnoPbRecord>(CollectionName, $"id_turno={idTurno}");
        return record != null ? MapearATurno(record) : null;
    }

    public async Task<List<Turno>> ObtenerPorProfesionalYFecha(Guid idProfesional, DateOnly fecha)
    {
        var profIdStr = idProfesional.ToString();
        var fechaStr = fecha.ToString("yyyy-MM-dd");

        // Obtenemos los turnos del profesional y filtramos por fecha
        var records = await _client.GetList<TurnoPbRecord>(CollectionName, $"id_profesional='{profIdStr}'");

        return records
            .Where(r => r.Fecha == fechaStr)
            .Select(MapearATurno)
            .ToList();
    }

    public async Task<List<Turno>> ObtenerPorPaciente(long idPaciente)
    {
        var records = await _client.GetList<TurnoPbRecord>(CollectionName, $"id_paciente={idPaciente}");
        return records.Select(MapearATurno).ToList();
    }

    public async Task<Turno> Crear(Turno turno)
    {
        if (turno.IdTurno <= 0)
        {
            var todos = await _client.GetList<TurnoPbRecord>(CollectionName);
            turno.IdTurno = todos.Any() ? todos.Max(t => t.IdTurno) + 1 : 1;
        }

        var payload = new
        {
            id_turno = turno.IdTurno,
            id_paciente = turno.IdPaciente,
            id_profesional = turno.IdProfesional.ToString(),
            fecha = turno.Fecha.ToString("yyyy-MM-dd"),
            hora = turno.Hora.ToString("HH:mm:ss"),
            estado = turno.Estado.ToString(),
            observacion = turno.Observacion
        };

        var created = await _client.Create<TurnoPbRecord>(CollectionName, payload);
        return MapearATurno(created);
    }

    public async Task<Turno> Actualizar(Turno turno)
    {
        var existing = await _client.GetFirst<TurnoPbRecord>(CollectionName, $"id_turno={turno.IdTurno}");
        if (existing == null)
        {
            return await Crear(turno);
        }

        var payload = new
        {
            id_paciente = turno.IdPaciente,
            id_profesional = turno.IdProfesional.ToString(),
            fecha = turno.Fecha.ToString("yyyy-MM-dd"),
            hora = turno.Hora.ToString("HH:mm:ss"),
            estado = turno.Estado.ToString(),
            observacion = turno.Observacion
        };

        var updated = await _client.Update<TurnoPbRecord>(CollectionName, existing.Id, payload);
        return MapearATurno(updated);
    }

    public async Task<bool> Eliminar(long idTurno)
    {
        var existing = await _client.GetFirst<TurnoPbRecord>(CollectionName, $"id_turno={idTurno}");
        if (existing == null) return false;

        return await _client.Delete(CollectionName, existing.Id);
    }

    private static Turno MapearATurno(TurnoPbRecord record)
    {
        DateOnly.TryParse(record.Fecha, out var fecha);
        TimeOnly.TryParse(record.Hora, out var hora);
        Enum.TryParse<EstadoTurno>(record.Estado, out var estado);
        Guid.TryParse(record.IdProfesional, out var idProf);

        return new Turno
        {
            IdTurno = record.IdTurno,
            CreatedAt = record.Created,
            Fecha = fecha,
            Hora = hora,
            Estado = estado,
            Observacion = record.Observacion,
            IdPaciente = record.IdPaciente,
            IdProfesional = idProf
        };
    }
}

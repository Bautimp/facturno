using System.Text.Json.Serialization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;

namespace Facturno.Infrastructure.PocketBase.Repositories;

internal class ObraSocialPbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_obra_social")]
    public long IdObraSocial { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class ObraSocialRepository : IObraSocialRepository
{
    private readonly PocketBaseClient _client;
    private const string CollectionName = "obras_sociales";

    public ObraSocialRepository(PocketBaseClient client)
    {
        _client = client;
    }

    public async Task<ObraSocial?> ObtenerPorId(long idObraSocial)
    {
        var record = await _client.GetFirst<ObraSocialPbRecord>(CollectionName, $"id_obra_social={idObraSocial}");
        return record != null ? MapearAObraSocial(record) : null;
    }

    public async Task<List<ObraSocial>> ObtenerTodas()
    {
        var records = await _client.GetList<ObraSocialPbRecord>(CollectionName, sort: "id_obra_social");
        return records.Select(MapearAObraSocial).ToList();
    }

    public async Task<ObraSocial> Crear(ObraSocial obraSocial)
    {
        // Calcular siguiente id_obra_social si no tiene
        if (obraSocial.IdObraSocial <= 0)
        {
            var todas = await ObtenerTodas();
            obraSocial.IdObraSocial = todas.Any() ? todas.Max(o => o.IdObraSocial) + 1 : 1;
        }

        var payload = new
        {
            id_obra_social = obraSocial.IdObraSocial,
            nombre = obraSocial.Nombre,
            activo = obraSocial.Activo
        };

        var created = await _client.Create<ObraSocialPbRecord>(CollectionName, payload);
        return MapearAObraSocial(created);
    }

    public async Task<ObraSocial> Actualizar(ObraSocial obraSocial)
    {
        var existing = await _client.GetFirst<ObraSocialPbRecord>(CollectionName, $"id_obra_social={obraSocial.IdObraSocial}");
        if (existing == null)
        {
            return await Crear(obraSocial);
        }

        var payload = new
        {
            nombre = obraSocial.Nombre,
            activo = obraSocial.Activo
        };

        var updated = await _client.Update<ObraSocialPbRecord>(CollectionName, existing.Id, payload);
        return MapearAObraSocial(updated);
    }

    public async Task<bool> CambiarEstadoActivo(long idObraSocial, bool activo)
    {
        var existing = await _client.GetFirst<ObraSocialPbRecord>(CollectionName, $"id_obra_social={idObraSocial}");
        if (existing == null) return false;

        await _client.Update<ObraSocialPbRecord>(CollectionName, existing.Id, new { activo = activo });
        return true;
    }

    private static ObraSocial MapearAObraSocial(ObraSocialPbRecord record)
    {
        return new ObraSocial
        {
            IdObraSocial = record.IdObraSocial,
            Nombre = record.Nombre,
            Activo = record.Activo,
            CreatedAt = record.Created
        };
    }
}

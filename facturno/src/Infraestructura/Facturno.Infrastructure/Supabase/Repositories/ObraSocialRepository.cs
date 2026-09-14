using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase.Repositories;

public class ObraSocialRepository : IObraSocialRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public ObraSocialRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<ObraSocial?> ObtenerPorIdAsync(long idObraSocial)
    {
        var res = await _supabaseClient.From<ObraSocialEntity>()
            .Where(x => x.IdObraSocial == idObraSocial)
            .Single();

        if (res == null) return null;

        return MapearAObraSocial(res);
    }

    public async Task<List<ObraSocial>> ObtenerTodasAsync()
    {
        var res = await _supabaseClient.From<ObraSocialEntity>().Get();
        return res.Models.Select(MapearAObraSocial).ToList();
    }

    public async Task<ObraSocial> CrearAsync(ObraSocial obraSocial)
    {
        var entity = new ObraSocialEntity
        {
            Nombre = obraSocial.Nombre,
            Activo = obraSocial.Activo
        };

        var created = (await _supabaseClient.From<ObraSocialEntity>().Insert(entity)).Models.First();
        return MapearAObraSocial(created);
    }

    public async Task<ObraSocial> ActualizarAsync(ObraSocial obraSocial)
    {
        var entity = new ObraSocialEntity
        {
            IdObraSocial = obraSocial.IdObraSocial,
            Nombre = obraSocial.Nombre,
            Activo = obraSocial.Activo
        };

        var updated = (await _supabaseClient.From<ObraSocialEntity>().Update(entity)).Models.First();
        return MapearAObraSocial(updated);
    }

    private static ObraSocial MapearAObraSocial(ObraSocialEntity entity)
    {
        return new ObraSocial
        {
            IdObraSocial = entity.IdObraSocial,
            CreatedAt = entity.CreatedAt,
            Nombre = entity.Nombre,
            Activo = entity.Activo
        };
    }
}

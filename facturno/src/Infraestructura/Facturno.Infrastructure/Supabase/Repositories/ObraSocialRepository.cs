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

    public async Task<ObraSocial?> ObtenerPorId(long idObraSocial)
    {
        var res = await _supabaseClient.From<ObraSocialEntity>()
            .Where(x => x.IdObraSocial == idObraSocial)
            .Single();

        if (res == null) return null;

        return MapearAObraSocial(res);
    }

    public async Task<List<ObraSocial>> ObtenerTodas()
    {
        var res = await _supabaseClient.From<ObraSocialEntity>().Get();
        if (!res.Models.Any())
        {
            var defaults = new List<ObraSocialEntity>
            {
                new ObraSocialEntity { IdObraSocial = 1, Nombre = "Particular", Activo = true },
                new ObraSocialEntity { IdObraSocial = 2, Nombre = "OSDE", Activo = true },
                new ObraSocialEntity { IdObraSocial = 3, Nombre = "Swiss Medical", Activo = true },
                new ObraSocialEntity { IdObraSocial = 4, Nombre = "Galeno", Activo = true }
            };

            foreach (var os in defaults)
            {
                try
                {
                    await _supabaseClient.From<ObraSocialEntity>().Insert(os);
                }
                catch
                {
                    // Ignorar si ya existe
                }
            }

            res = await _supabaseClient.From<ObraSocialEntity>().Get();
        }

        return res.Models.Select(MapearAObraSocial).ToList();
    }

    public async Task<ObraSocial> Crear(ObraSocial obraSocial)
    {
        var entity = new ObraSocialEntity
        {
            Nombre = obraSocial.Nombre,
            Activo = obraSocial.Activo
        };

        var created = (await _supabaseClient.From<ObraSocialEntity>().Insert(entity)).Models.First();
        return MapearAObraSocial(created);
    }

    public async Task<ObraSocial> Actualizar(ObraSocial obraSocial)
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

    public async Task<bool> CambiarEstadoActivo(long idObraSocial, bool activo)
    {
        var existing = await _supabaseClient.From<ObraSocialEntity>()
            .Where(x => x.IdObraSocial == idObraSocial)
            .Single();

        if (existing == null) return false;

        existing.Activo = activo;
        await _supabaseClient.From<ObraSocialEntity>().Update(existing);
        return true;
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

using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase;

public class TurnoRepository : ITurnoRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    // Inyectamos el cliente de Supabase
    public TurnoRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<List<Turno>> ObtenerTodosAsync()
    {
        // 1. Buscamos los datos en Supabase
        var response = await _supabaseClient.From<TurnoEntity>().Get();
        var entidades = response.Models;

        // 2. Mapeamos las entidades de BD a los modelos de dominio (Shared)
        return entidades.Select(e => new Turno
        {
            IdTurno = e.IdTurno,
            Fecha = DateOnly.FromDateTime(e.Fecha),
            Hora = TimeOnly.FromTimeSpan(e.Hora),
            Estado = Enum.Parse<EstadoTurno>(e.Estado),
            Observaciones = e.Observaciones,
            IdPaciente = e.IdPaciente,
            IdProfesional = e.IdProfesional
        }).ToList();
    }

    public async Task<Turno?> ObtenerPorIdAsync(int id)
    {
        var response = await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdTurno == id)
            .Single();

        if (response == null) return null;

        return new Turno
        {
            IdTurno = response.IdTurno,
            Fecha = DateOnly.FromDateTime(response.Fecha),
            Hora = TimeOnly.FromTimeSpan(response.Hora),
            Estado = Enum.Parse<EstadoTurno>(response.Estado),
            Observaciones = response.Observaciones,
            IdPaciente = response.IdPaciente,
            IdProfesional = response.IdProfesional
        };
    }

    public async Task<Turno> CrearAsync(Turno turno)
    {
        // 1. Mapeamos del dominio a la entidad de BD
        var entidad = new TurnoEntity
        {
            Fecha = turno.Fecha.ToDateTime(TimeOnly.MinValue),
            Hora = turno.Hora.ToTimeSpan(),
            Estado = turno.Estado.ToString(),
            Observaciones = turno.Observaciones,
            IdPaciente = turno.IdPaciente,
            IdProfesional = turno.IdProfesional
        };

        // 2. Insertamos en Supabase
        var response = await _supabaseClient.From<TurnoEntity>().Insert(entidad);
        var entidadInsertada = response.Models.First();

        // 3. Actualizamos el ID generado por la base de datos
        turno.IdTurno = entidadInsertada.IdTurno;
        return turno;
    }

    public async Task EliminarAsync(int id)
    {
        await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdTurno == id)
            .Delete();
    }
}
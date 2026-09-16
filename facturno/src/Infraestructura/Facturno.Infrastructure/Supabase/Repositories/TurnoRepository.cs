using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase.Repositories;

public class TurnoRepository : ITurnoRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public TurnoRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Turno?> ObtenerPorIdAsync(long idTurno)
    {
        var response = await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdTurno == idTurno)
            .Single();

        if (response == null) return null;

        return MapearATurno(response);
    }

    public async Task<List<Turno>> ObtenerPorProfesionalYFechaAsync(Guid idProfesional, DateOnly fecha)
    {
        var profIdStr = idProfesional.ToString();
        var response = await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdProfesional == profIdStr)
            .Get();

        var targetDate = fecha.ToDateTime(TimeOnly.MinValue).Date;

        return response.Models
            .Where(x => x.Fecha.Date == targetDate)
            .Select(MapearATurno)
            .ToList();
    }

    public async Task<List<Turno>> ObtenerPorPacienteAsync(long idPaciente)
    {
        var response = await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdPaciente == idPaciente)
            .Get();

        return response.Models.Select(MapearATurno).ToList();
    }

    public async Task<Turno> CrearAsync(Turno turno)
    {
        var entity = new TurnoEntity
        {
            IdPaciente = turno.IdPaciente,
            IdProfesional = turno.IdProfesional.ToString(),
            Fecha = turno.Fecha.ToDateTime(TimeOnly.MinValue),
            Hora = turno.Hora.ToTimeSpan(),
            Estado = turno.Estado.ToString(),
            Observacion = turno.Observacion
        };

        var response = await _supabaseClient.From<TurnoEntity>().Insert(entity);
        var creada = response.Models.First();
        return MapearATurno(creada);
    }

    public async Task<Turno> ActualizarAsync(Turno turno)
    {
        var entity = new TurnoEntity
        {
            IdTurno = turno.IdTurno,
            IdPaciente = turno.IdPaciente,
            IdProfesional = turno.IdProfesional.ToString(),
            Fecha = turno.Fecha.ToDateTime(TimeOnly.MinValue),
            Hora = turno.Hora.ToTimeSpan(),
            Estado = turno.Estado.ToString(),
            Observacion = turno.Observacion
        };

        var response = await _supabaseClient.From<TurnoEntity>().Update(entity);
        var actualizada = response.Models.First();
        return MapearATurno(actualizada);
    }

    public async Task<bool> EliminarAsync(long idTurno)
    {
        await _supabaseClient.From<TurnoEntity>()
            .Where(x => x.IdTurno == idTurno)
            .Delete();
        return true;
    }

    private static Turno MapearATurno(TurnoEntity entity)
    {
        return new Turno
        {
            IdTurno = entity.IdTurno,
            CreatedAt = entity.CreatedAt,
            Fecha = DateOnly.FromDateTime(entity.Fecha),
            Hora = TimeOnly.FromTimeSpan(entity.Hora),
            Estado = Enum.TryParse<EstadoTurno>(entity.Estado, out var estado) ? estado : EstadoTurno.Activo,
            Observacion = entity.Observacion,
            IdPaciente = entity.IdPaciente,
            IdProfesional = Guid.TryParse(entity.IdProfesional, out var idProf) ? idProf : Guid.Empty
        };
    }
}

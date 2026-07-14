using Facturno.Shared.Models;

namespace Facturno.Shared.Interfaces;

public interface ITurnoRepository
{
    Task<List<Turno>> ObtenerTodosAsync();
    Task<Turno?> ObtenerPorIdAsync(int id);
    Task<Turno> CrearAsync(Turno turno);
    Task EliminarAsync(int id);
}
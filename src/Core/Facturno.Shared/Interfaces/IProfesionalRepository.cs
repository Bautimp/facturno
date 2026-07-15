using Facturno.Shared.Models;

namespace Facturno.Shared.Interfaces;

public interface IProfesionalRepository
{
    Task<List<Profesional>> ObtenerTodosAsync();
    Task<Profesional?> ObtenerPorIdAsync(int id);
    Task<Profesional> CrearAsync(Profesional profesional);
}
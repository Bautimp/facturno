using Facturno.Shared.Models;

namespace Facturno.Shared.Interfaces;

public interface IPacienteRepository
{
    Task<List<Paciente>> ObtenerTodosAsync();
    Task<Paciente?> ObtenerPorIdAsync(int id);
    Task<Paciente> CrearAsync(Paciente paciente);
}
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Interfaces;

// Interfaz de servicio para los turnos
public interface ITurnoService
{
    Task<ApiResponse<Turno>> AgendarTurno(TurnoCreateDto dto);
    Task<ApiResponse<List<Turno>>> AgendarTurnosRecurrentes(TurnoRecurrenteCreateDto dto);
    Task<ApiResponse<Turno>> ActualizarTurno(TurnoUpdateDto dto);
    Task<ApiResponse<bool>> CancelarTurno(long idTurno);
    Task<ApiResponse<List<Turno>>> ListarPorProfesionalYFecha(Guid idProfesional, DateOnly fecha);
}

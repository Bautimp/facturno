using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Interfaces;

//Interfaz de servicio para los turnos
public interface ITurnoService
{
    Task<ApiResponse<Turno>> AgendarTurnoAsync(TurnoCreateDto dto);
    Task<ApiResponse<List<Turno>>> AgendarTurnosRecurrentesAsync(TurnoRecurrenteCreateDto dto);
    Task<ApiResponse<Turno>> ActualizarTurnoAsync(TurnoUpdateDto dto);
    Task<ApiResponse<bool>> CancelarTurnoAsync(long idTurno);
    Task<ApiResponse<List<Turno>>> ListarPorProfesionalYFechaAsync(Guid idProfesional, DateOnly fecha);
}

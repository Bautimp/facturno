using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.API.Services;

//Implementación de la interfaz de servicio para los turnos
public class TurnoService : ITurnoService
{
    private readonly ITurnoRepository _turnoRepository;

    public TurnoService(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    public async Task<ApiResponse<Turno>> AgendarTurnoAsync(TurnoCreateDto dto)
    {
        // Validar solapamiento de turnos (30 minutos mínimos)
        var turnosDelDia = await _turnoRepository.ObtenerPorProfesionalYFechaAsync(dto.IdProfesional, dto.Fecha);

        var horaInicio = dto.Hora;
        var horaFin = dto.Hora.Add(TimeSpan.FromMinutes(30));

        bool existeSolapamiento = turnosDelDia.Any(t =>
            t.Estado != EstadoTurno.Cancelado &&
            ((t.Hora >= horaInicio && t.Hora < horaFin) ||
             (t.Hora.Add(TimeSpan.FromMinutes(30)) > horaInicio && t.Hora <= horaInicio)));

        if (existeSolapamiento)
        {
            return ApiResponse<Turno>.Error("El profesional ya cuenta con un turno asignado dentro del rango de 30 minutos en dicho horario.");
        }

        // Crear entidad Turno
        var nuevoTurno = new Turno
        {
            Fecha = dto.Fecha,
            Hora = dto.Hora,
            IdPaciente = dto.IdPaciente,
            IdProfesional = dto.IdProfesional,
            Observacion = dto.Observacion,
            Estado = EstadoTurno.Activo
        };

        var turnoCreado = await _turnoRepository.CrearAsync(nuevoTurno);
        return ApiResponse<Turno>.Ok(turnoCreado, "Turno agendado exitosamente.");
    }

    public async Task<ApiResponse<List<Turno>>> AgendarTurnosRecurrentesAsync(TurnoRecurrenteCreateDto dto)
    {
        var turnosCreados = new List<Turno>();
        var erroresSolapamiento = new List<string>();

        var fechaActual = dto.FechaInicio;
        while (fechaActual <= dto.FechaFin)
        {
            if (fechaActual.DayOfWeek == dto.DiaSemana)
            {
                var createDto = new TurnoCreateDto
                {
                    Fecha = fechaActual,
                    Hora = dto.Hora,
                    IdPaciente = dto.IdPaciente,
                    IdProfesional = dto.IdProfesional,
                    Observacion = dto.Observacion
                };

                var resultado = await AgendarTurnoAsync(createDto);
                if (resultado.Exito && resultado.Datos != null)
                {
                    turnosCreados.Add(resultado.Datos);
                }
                else
                {
                    erroresSolapamiento.Add($"Conflicto en la fecha {fechaActual:dd/MM/yyyy}: {resultado.Mensaje}");
                }
            }

            fechaActual = fechaActual.AddDays(1);
        }

        var response = ApiResponse<List<Turno>>.Ok(turnosCreados, $"Se agendaron {turnosCreados.Count} turnos recurrentes.");
        if (erroresSolapamiento.Any())
        {
            response.Errores = erroresSolapamiento;
        }

        return response;
    }

    public async Task<ApiResponse<Turno>> ActualizarTurnoAsync(TurnoUpdateDto dto)
    {
        var turnoExistente = await _turnoRepository.ObtenerPorIdAsync(dto.IdTurno);
        if (turnoExistente == null)
        {
            return ApiResponse<Turno>.Error("El turno especificado no existe.");
        }

        turnoExistente.Fecha = dto.Fecha;
        turnoExistente.Hora = dto.Hora;
        turnoExistente.Estado = dto.Estado;
        turnoExistente.Observacion = dto.Observacion;

        var actualizado = await _turnoRepository.ActualizarAsync(turnoExistente);
        return ApiResponse<Turno>.Ok(actualizado, "Turno actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> CancelarTurnoAsync(long idTurno)
    {
        var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno);
        if (turno == null)
        {
            return ApiResponse<bool>.Error("Turno no encontrado.");
        }

        turno.Estado = EstadoTurno.Cancelado;
        await _turnoRepository.ActualizarAsync(turno);
        return ApiResponse<bool>.Ok(true, "Turno cancelado exitosamente.");
    }

    public async Task<ApiResponse<List<Turno>>> ListarPorProfesionalYFechaAsync(Guid idProfesional, DateOnly fecha)
    {
        var turnos = await _turnoRepository.ObtenerPorProfesionalYFechaAsync(idProfesional, fecha);
        return ApiResponse<List<Turno>>.Ok(turnos);
    }
}

using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.API.Services;

// Implementación de la interfaz de servicio para los turnos
public class TurnoService : ITurnoService
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IConfiguracionService _configService;

    public TurnoService(ITurnoRepository turnoRepository, IConfiguracionService configService)
    {
        _turnoRepository = turnoRepository;
        _configService = configService;
    }

    public async Task<ApiResponse<Turno>> AgendarTurno(TurnoCreateDto dto)
    {
        var config = _configService.ObtenerConfiguracion();

        // 1. Validar rango horario de atención
        var horaTurno = dto.Hora;
        if (horaTurno < config.HoraInicio || horaTurno > config.HoraFin)
        {
            return ApiResponse<Turno>.Error($"El horario del turno ({dto.Hora:HH\\:mm}) está fuera del horario de atención configurado ({config.HoraInicioTurnos} a {config.HoraFinTurnos}).");
        }

        var turnosDelDia = await _turnoRepository.ObtenerPorProfesionalYFecha(dto.IdProfesional, dto.Fecha);

        // 2. Validar límite diario de pacientes por profesional
        int turnosActivosDelDia = turnosDelDia.Count(t => t.Estado != EstadoTurno.Cancelado);
        if (turnosActivosDelDia >= config.MaxPacientesPorDiaPorProfesional)
        {
            return ApiResponse<Turno>.Error($"Se ha alcanzado el límite máximo diario de pacientes para este profesional ({config.MaxPacientesPorDiaPorProfesional} pacientes).");
        }

        // 3. Validar solapamiento de turnos según intervalo configurado
        var intervalo = TimeSpan.FromMinutes(config.MinutosEntreTurnos);
        var horaInicio = dto.Hora;
        var horaFin = dto.Hora.Add(intervalo);

        bool existeSolapamiento = turnosDelDia.Any(t =>
            t.Estado != EstadoTurno.Cancelado &&
            ((t.Hora >= horaInicio && t.Hora < horaFin) ||
             (t.Hora.Add(intervalo) > horaInicio && t.Hora <= horaInicio)));

        if (existeSolapamiento)
        {
            return ApiResponse<Turno>.Error($"El profesional ya cuenta con un turno asignado dentro del rango de {config.MinutosEntreTurnos} minutos en dicho horario.");
        }

        string? enlace = null;
        if (dto.Modalidad == ModalidadTurno.Online && !string.IsNullOrWhiteSpace(dto.EnlaceReunion))
        {
            enlace = dto.EnlaceReunion.Trim();
        }

        // Crear entidad Turno
        var nuevoTurno = new Turno
        {
            Fecha = dto.Fecha,
            Hora = dto.Hora,
            IdPaciente = dto.IdPaciente,
            IdProfesional = dto.IdProfesional,
            Observacion = dto.Observacion,
            Estado = EstadoTurno.Activo,
            Modalidad = dto.Modalidad,
            EnlaceReunion = enlace
        };

        var turnoCreado = await _turnoRepository.Crear(nuevoTurno);
        return ApiResponse<Turno>.Ok(turnoCreado, "Turno agendado exitosamente.");
    }

    public async Task<ApiResponse<List<Turno>>> AgendarTurnosRecurrentes(TurnoRecurrenteCreateDto dto)
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
                    Observacion = dto.Observacion,
                    Modalidad = dto.Modalidad,
                    EnlaceReunion = dto.EnlaceReunion
                };

                var resultado = await AgendarTurno(createDto);
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

    public async Task<ApiResponse<Turno>> ActualizarTurno(TurnoUpdateDto dto)
    {
        var turnoExistente = await _turnoRepository.ObtenerPorId(dto.IdTurno);
        if (turnoExistente == null)
        {
            return ApiResponse<Turno>.Error("El turno especificado no existe.");
        }

        // Si se modifica la fecha u hora y el nuevo estado no es Cancelado, validar solapamientos y límites
        if (dto.Estado != EstadoTurno.Cancelado)
        {
            var config = _configService.ObtenerConfiguracion();

            var horaTurno = dto.Hora;
            if (horaTurno < config.HoraInicio || horaTurno > config.HoraFin)
            {
                return ApiResponse<Turno>.Error($"El horario del turno ({dto.Hora:HH\\:mm}) está fuera del horario de atención configurado ({config.HoraInicioTurnos} a {config.HoraFinTurnos}).");
            }

            if (dto.Fecha != turnoExistente.Fecha || dto.Hora != turnoExistente.Hora)
            {
                var turnosDelDia = await _turnoRepository.ObtenerPorProfesionalYFecha(turnoExistente.IdProfesional, dto.Fecha);

                // Si se cambió de fecha, verificar límite diario de pacientes
                if (dto.Fecha != turnoExistente.Fecha)
                {
                    int turnosActivosDelDia = turnosDelDia.Count(t => t.IdTurno != dto.IdTurno && t.Estado != EstadoTurno.Cancelado);
                    if (turnosActivosDelDia >= config.MaxPacientesPorDiaPorProfesional)
                    {
                        return ApiResponse<Turno>.Error($"Se ha alcanzado el límite máximo diario de pacientes para este profesional ({config.MaxPacientesPorDiaPorProfesional} pacientes) en la fecha seleccionada.");
                    }
                }

                var intervalo = TimeSpan.FromMinutes(config.MinutosEntreTurnos);
                var horaInicio = dto.Hora;
                var horaFin = dto.Hora.Add(intervalo);

                bool existeSolapamiento = turnosDelDia.Any(t =>
                    t.IdTurno != dto.IdTurno &&
                    t.Estado != EstadoTurno.Cancelado &&
                    ((t.Hora >= horaInicio && t.Hora < horaFin) ||
                     (t.Hora.Add(intervalo) > horaInicio && t.Hora <= horaInicio)));

                if (existeSolapamiento)
                {
                    return ApiResponse<Turno>.Error($"El profesional ya cuenta con un turno asignado dentro del rango de {config.MinutosEntreTurnos} minutos en dicho horario.");
                }
            }
        }

        turnoExistente.Fecha = dto.Fecha;
        turnoExistente.Hora = dto.Hora;
        turnoExistente.Estado = dto.Estado;
        turnoExistente.Observacion = dto.Observacion;
        turnoExistente.Modalidad = dto.Modalidad;

        if (dto.Modalidad == ModalidadTurno.Online)
        {
            turnoExistente.EnlaceReunion = !string.IsNullOrWhiteSpace(dto.EnlaceReunion)
                ? dto.EnlaceReunion.Trim()
                : null;
        }
        else
        {
            turnoExistente.EnlaceReunion = null;
        }

        var actualizado = await _turnoRepository.Actualizar(turnoExistente);
        return ApiResponse<Turno>.Ok(actualizado, "Turno actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> CancelarTurno(long idTurno)
    {
        var turno = await _turnoRepository.ObtenerPorId(idTurno);
        if (turno == null)
        {
            return ApiResponse<bool>.Error("Turno no encontrado.");
        }

        turno.Estado = EstadoTurno.Cancelado;
        await _turnoRepository.Actualizar(turno);
        return ApiResponse<bool>.Ok(true, "Turno cancelado exitosamente.");
    }

    public async Task<ApiResponse<List<Turno>>> ListarPorProfesionalYFecha(Guid idProfesional, DateOnly fecha)
    {
        var turnos = await _turnoRepository.ObtenerPorProfesionalYFecha(idProfesional, fecha);
        return ApiResponse<List<Turno>>.Ok(turnos);
    }

    public async Task<ApiResponse<List<Turno>>> ListarPorProfesionalYRango(Guid idProfesional, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var turnos = await _turnoRepository.ObtenerPorProfesionalYRango(idProfesional, fechaInicio, fechaFin);
        return ApiResponse<List<Turno>>.Ok(turnos);
    }
}

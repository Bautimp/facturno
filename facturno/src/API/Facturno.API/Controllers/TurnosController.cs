using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;

namespace Facturno.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TurnosController : ControllerBase
{
    private readonly ITurnoService _turnoService;

    public TurnosController(ITurnoService turnoService)
    {
        _turnoService = turnoService;
    }

    [HttpGet("profesional/{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<List<Turno>>>> ObtenerPorProfesionalYFecha(
        Guid idProfesional, 
        [FromQuery] string? fecha = null,
        [FromQuery] string? fechaInicio = null,
        [FromQuery] string? fechaFin = null)
    {
        try
        {
            // Consulta de rango optimizada (1 sola petición para toda la semana/mes)
            if (!string.IsNullOrWhiteSpace(fechaInicio) && !string.IsNullOrWhiteSpace(fechaFin))
            {
                if (!DateOnly.TryParse(fechaInicio, out var inicio) || !DateOnly.TryParse(fechaFin, out var fin))
                {
                    return BadRequest(ApiResponse<List<Turno>>.Error("Formato de fecha inválido en el rango. Usar YYYY-MM-DD."));
                }

                var respuestaRango = await _turnoService.ListarPorProfesionalYRango(idProfesional, inicio, fin);
                return Ok(respuestaRango);
            }

            // Consulta por fecha individual
            if (!string.IsNullOrWhiteSpace(fecha))
            {
                if (!DateOnly.TryParse(fecha, out var dateOnly))
                {
                    return BadRequest(ApiResponse<List<Turno>>.Error("Formato de fecha inválido. Usar YYYY-MM-DD."));
                }

                var respuesta = await _turnoService.ListarPorProfesionalYFecha(idProfesional, dateOnly);
                return Ok(respuesta);
            }

            return BadRequest(ApiResponse<List<Turno>>.Error("Debe especificar una 'fecha' o un rango con 'fechaInicio' y 'fechaFin'."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Turno>>.Error($"Error al obtener turnos: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Turno>>> AgendarTurno([FromBody] TurnoCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<Turno>.Error("Datos de solicitud inválidos."));
            }

            var respuesta = await _turnoService.AgendarTurno(dto);
            if (!respuesta.Exito)
            {
                return BadRequest(respuesta);
            }

            return CreatedAtAction(nameof(ObtenerPorProfesionalYFecha), new { idProfesional = respuesta.Datos!.IdProfesional, fecha = respuesta.Datos.Fecha.ToString("yyyy-MM-dd") }, respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Turno>.Error($"Error al agendar turno: {ex.Message}"));
        }
    }

    [HttpPost("recurrentes")]
    public async Task<ActionResult<ApiResponse<List<Turno>>>> AgendarTurnosRecurrentes([FromBody] TurnoRecurrenteCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<List<Turno>>.Error("Datos de solicitud inválidos."));
            }

            var respuesta = await _turnoService.AgendarTurnosRecurrentes(dto);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Turno>>.Error($"Error al agendar turnos recurrentes: {ex.Message}"));
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<Turno>>> ActualizarTurno([FromBody] TurnoUpdateDto dto)
    {
        try
        {
            var respuesta = await _turnoService.ActualizarTurno(dto);
            if (!respuesta.Exito)
            {
                return BadRequest(respuesta);
            }

            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Turno>.Error($"Error al actualizar turno: {ex.Message}"));
        }
    }

    [HttpDelete("{idTurno:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelarTurno(long idTurno)
    {
        try
        {
            var respuesta = await _turnoService.CancelarTurno(idTurno);
            if (!respuesta.Exito)
            {
                return NotFound(respuesta);
            }

            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Error($"Error al cancelar turno: {ex.Message}"));
        }
    }
}

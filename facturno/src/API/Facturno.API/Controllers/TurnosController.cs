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
    public async Task<ActionResult<ApiResponse<List<Turno>>>> ObtenerPorProfesionalYFecha(Guid idProfesional, [FromQuery] string fecha)
    {
        try
        {
            if (!DateOnly.TryParse(fecha, out var dateOnly))
            {
                return BadRequest(ApiResponse<List<Turno>>.Error("Formato de fecha inválido. Usar YYYY-MM-DD."));
            }

            var respuesta = await _turnoService.ListarPorProfesionalYFechaAsync(idProfesional, dateOnly);
            return Ok(respuesta);
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

            var respuesta = await _turnoService.AgendarTurnoAsync(dto);
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

            var respuesta = await _turnoService.AgendarTurnosRecurrentesAsync(dto);
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
            var respuesta = await _turnoService.ActualizarTurnoAsync(dto);
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
            var respuesta = await _turnoService.CancelarTurnoAsync(idTurno);
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

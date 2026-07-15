using Microsoft.AspNetCore.Mvc;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.API.Controllers;

[ApiController]
[Route("api/[controller]")] // La ruta final será "api/turnos"
public class TurnosController : ControllerBase
{
    private readonly ITurnoRepository _turnoRepository;

    // Aquí ocurre la "magia" de la Inyección de Dependencias
    public TurnosController(ITurnoRepository turnoRepository)
    {
        _turnoRepository = turnoRepository;
    }

    // GET: api/turnos
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        try
        {
            var turnos = await _turnoRepository.ObtenerTodosAsync();
            return Ok(turnos); // Devuelve un JSON con status 200 OK
        }
        catch (Exception ex)
        {
            // En producción aquí usarías un ILogger
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    // GET: api/turnos/5
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var turno = await _turnoRepository.ObtenerPorIdAsync(id);
        
        if (turno == null) 
            return NotFound(); // Devuelve un status 404 Not Found

        return Ok(turno);
    }

    // POST: api/turnos
    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] TurnoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Devuelve 400 si faltan datos
        }

        try
        {
            // 1. Mapeamos el DTO de entrada a nuestra Entidad de Dominio
            var nuevoTurno = new Turno
            {
                Fecha = dto.Fecha,
                Hora = dto.Hora,
                IdPaciente = dto.IdPaciente,
                IdProfesional = dto.IdProfesional,
                Observaciones = dto.Observaciones,
                Estado = EstadoTurno.Pendiente // Todos los turnos nuevos nacen pendientes
            };

            // 2. Le pedimos al repositorio que guarde en Supabase
            var turnoCreado = await _turnoRepository.CrearAsync(nuevoTurno);

            // 3. Devolvemos 201 Created indicando la URL donde se puede consultar el nuevo turno
            return CreatedAtAction(nameof(ObtenerPorId), new { id = turnoCreado.IdTurno }, turnoCreado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el turno: {ex.Message}");
        }
    }
}
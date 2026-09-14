using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;

namespace Facturno.API.Controllers;

[Authorize(Roles = "Profesional,Administrativo,Operador")]
[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteRepository _pacienteRepository;

    public PacientesController(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Paciente>>>> ObtenerTodos()
    {
        var pacientes = await _pacienteRepository.ObtenerTodosAsync();
        return Ok(ApiResponse<List<Paciente>>.Ok(pacientes));
    }

    [HttpGet("{idPaciente:long}")]
    public async Task<ActionResult<ApiResponse<Paciente>>> ObtenerPorId(long idPaciente)
    {
        var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente);
        if (paciente == null)
        {
            return NotFound(ApiResponse<Paciente>.Error("Paciente no encontrado."));
        }
        return Ok(ApiResponse<Paciente>.Ok(paciente));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Paciente>>> CrearPaciente([FromBody] PacienteCreateDto dto)
    {
        var persona = new Persona
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            Telefono = dto.Telefono
        };

        var paciente = new Paciente
        {
            NumDocumento = dto.NumDocumento,
            TipoDocumento = dto.TipoDocumento,
            IdObraSocial = dto.IdObraSocial,
            NumObraSocial = dto.NumObraSocial,
            PorcentajeIva = dto.PorcentajeIva
        };

        var pacienteCreado = await _pacienteRepository.CrearAsync(paciente, persona);
        return CreatedAtAction(nameof(ObtenerPorId), new { idPaciente = pacienteCreado.IdPaciente }, ApiResponse<Paciente>.Ok(pacienteCreado, "Paciente creado con éxito."));
    }
}

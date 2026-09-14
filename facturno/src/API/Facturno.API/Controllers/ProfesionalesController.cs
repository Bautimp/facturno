using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;

namespace Facturno.API.Controllers;

[Authorize(Roles = "Operador")]
[ApiController]
[Route("api/[controller]")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalRepository _profesionalRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ProfesionalesController(IProfesionalRepository profesionalRepository, IUsuarioRepository usuarioRepository)
    {
        _profesionalRepository = profesionalRepository;
        _usuarioRepository = usuarioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Profesional>>>> ObtenerTodos()
    {
        var profesionales = await _profesionalRepository.ObtenerTodosAsync();
        return Ok(ApiResponse<List<Profesional>>.Ok(profesionales));
    }

    [HttpGet("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> ObtenerPorId(Guid idProfesional)
    {
        var profesional = await _profesionalRepository.ObtenerPorIdAsync(idProfesional);
        if (profesional == null)
        {
            return NotFound(ApiResponse<Profesional>.Error("Profesional no encontrado."));
        }
        return Ok(ApiResponse<Profesional>.Ok(profesional));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Profesional>>> Crear([FromBody] ProfesionalCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Profesional>.Error("Datos de entrada inválidos."));
        }

        var persona = new Persona
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo,
            Telefono = dto.Telefono
        };

        var nuevoIdUsuario = Guid.NewGuid();

        var usuario = new Usuario
        {
            IdUsuario = nuevoIdUsuario,
            Rol = RolUsuario.Profesional,
            Activo = true
        };

        var profesional = new Profesional
        {
            IdProfesional = nuevoIdUsuario,
            Especialidad = dto.Especialidad,
            Matricula = dto.Matricula,
            Cuit = dto.Cuit,
            PrecioConsulta = dto.PrecioConsulta,
            TipoComprobante = dto.TipoComprobante,
            CondicionIva = dto.CondicionIva
        };

        var creado = await _profesionalRepository.CrearAsync(profesional, usuario, persona);
        return CreatedAtAction(nameof(ObtenerPorId), new { idProfesional = creado.IdProfesional }, ApiResponse<Profesional>.Ok(creado, "Profesional creado correctamente."));
    }

    [HttpPut("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> Actualizar(Guid idProfesional, [FromBody] ProfesionalUpdateDto dto)
    {
        var existente = await _profesionalRepository.ObtenerPorIdAsync(idProfesional);
        if (existente == null)
        {
            return NotFound(ApiResponse<Profesional>.Error("Profesional no encontrado."));
        }

        var persona = new Persona
        {
            IdPersona = existente.Usuario?.IdPersona ?? 0,
            Nombre = dto.Nombre ?? existente.Usuario?.Persona?.Nombre ?? string.Empty,
            Apellido = dto.Apellido ?? existente.Usuario?.Persona?.Apellido ?? string.Empty,
            Correo = dto.Correo ?? existente.Usuario?.Persona?.Correo,
            Telefono = dto.Telefono ?? existente.Usuario?.Persona?.Telefono
        };

        var profesional = new Profesional
        {
            IdProfesional = idProfesional,
            Especialidad = dto.Especialidad ?? existente.Especialidad,
            Matricula = dto.Matricula ?? existente.Matricula,
            Cuit = dto.Cuit ?? existente.Cuit,
            PrecioConsulta = dto.PrecioConsulta ?? existente.PrecioConsulta,
            TipoComprobante = dto.TipoComprobante ?? existente.TipoComprobante,
            CondicionIva = dto.CondicionIva ?? existente.CondicionIva
        };

        var actualizado = await _profesionalRepository.ActualizarAsync(profesional, persona);
        return Ok(ApiResponse<Profesional>.Ok(actualizado, "Profesional actualizado correctamente."));
    }

    [HttpPatch("{idProfesional:guid}/activo")]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoActivo(Guid idProfesional, [FromQuery] bool activo)
    {
        var resultado = await _usuarioRepository.CambiarEstadoActivoAsync(idProfesional, activo);
        if (!resultado)
        {
            return NotFound(ApiResponse<bool>.Error("No se pudo encontrar el usuario del profesional."));
        }

        var mensaje = activo ? "Profesional activado correctamente." : "Profesional desactivado correctamente. Turnos activos cancelados por sistema.";
        return Ok(ApiResponse<bool>.Ok(true, mensaje));
    }
}

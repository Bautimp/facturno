using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;

namespace Facturno.API.Controllers;

[Authorize(Roles = "Profesional,Administrativo,Operador")]
[ApiController]
[Route("api/[controller]")]
public class ObrasSocialesController : ControllerBase
{
    private readonly IObraSocialRepository _obraSocialRepository;

    public ObrasSocialesController(IObraSocialRepository obraSocialRepository)
    {
        _obraSocialRepository = obraSocialRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ObraSocial>>>> ObtenerTodas()
    {
        try
        {
            var result = await _obraSocialRepository.ObtenerTodasAsync();
            return Ok(ApiResponse<List<ObraSocial>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ObraSocial>>.Error($"Error al obtener obras sociales: {ex.Message}"));
        }
    }

    [HttpGet("{idObraSocial:long}")]
    public async Task<ActionResult<ApiResponse<ObraSocial>>> ObtenerPorId(long idObraSocial)
    {
        try
        {
            var result = await _obraSocialRepository.ObtenerPorIdAsync(idObraSocial);
            if (result == null)
            {
                return NotFound(ApiResponse<ObraSocial>.Error("Obra Social no encontrada."));
            }
            return Ok(ApiResponse<ObraSocial>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ObraSocial>.Error($"Error al obtener obra social: {ex.Message}"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Operador,Administrativo")]
    public async Task<ActionResult<ApiResponse<ObraSocial>>> Crear([FromBody] ObraSocial obraSocial)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(obraSocial.Nombre))
            {
                return BadRequest(ApiResponse<ObraSocial>.Error("El nombre de la obra social es obligatorio."));
            }

            var creada = await _obraSocialRepository.CrearAsync(obraSocial);
            return Ok(ApiResponse<ObraSocial>.Ok(creada, "Obra Social creada con éxito."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ObraSocial>.Error($"Error al crear obra social: {ex.Message}"));
        }
    }

    [HttpPut("{idObraSocial:long}")]
    [Authorize(Roles = "Operador,Administrativo")]
    public async Task<ActionResult<ApiResponse<ObraSocial>>> Actualizar(long idObraSocial, [FromBody] ObraSocial obraSocial)
    {
        try
        {
            if (idObraSocial != obraSocial.IdObraSocial)
            {
                obraSocial.IdObraSocial = idObraSocial;
            }

            if (string.IsNullOrWhiteSpace(obraSocial.Nombre))
            {
                return BadRequest(ApiResponse<ObraSocial>.Error("El nombre de la obra social es obligatorio."));
            }

            var actualizada = await _obraSocialRepository.ActualizarAsync(obraSocial);
            return Ok(ApiResponse<ObraSocial>.Ok(actualizada, "Obra Social actualizada con éxito."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ObraSocial>.Error($"Error al actualizar obra social: {ex.Message}"));
        }
    }

    [HttpPatch("{idObraSocial:long}/activo")]
    [Authorize(Roles = "Operador,Administrativo")]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoActivo(long idObraSocial, [FromQuery] bool activo)
    {
        try
        {
            var exito = await _obraSocialRepository.CambiarEstadoActivoAsync(idObraSocial, activo);
            if (!exito)
            {
                return NotFound(ApiResponse<bool>.Error("Obra Social no encontrada."));
            }
            string msg = activo ? "Obra Social activada." : "Obra Social dada de baja.";
            return Ok(ApiResponse<bool>.Ok(true, msg));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Error($"Error al cambiar estado de obra social: {ex.Message}"));
        }
    }
}

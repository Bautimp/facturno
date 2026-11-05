using Facturno.Shared.DTOs;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Facturno.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionService _configuracionService;

    public ConfiguracionController(IConfiguracionService configuracionService)
    {
        _configuracionService = configuracionService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<ConfiguracionSistema>> ObtenerConfiguracion()
    {
        try
        {
            var config = _configuracionService.ObtenerConfiguracion();
            return Ok(ApiResponse<ConfiguracionSistema>.Ok(config));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ConfiguracionSistema>.Error($"Error al obtener la configuración: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Operador")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ConfiguracionSistema>>> ActualizarConfiguracion([FromBody] ConfiguracionSistemaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ConfiguracionSistema>.Error("Datos de configuración inválidos."));
            }

            var respuesta = await _configuracionService.ActualizarConfiguracion(dto);
            if (!respuesta.Exito)
            {
                return BadRequest(respuesta);
            }

            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ConfiguracionSistema>.Error($"Error al actualizar la configuración: {ex.Message}"));
        }
    }
}


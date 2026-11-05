using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Interfaces;

public interface IConfiguracionService
{
    ConfiguracionSistema ObtenerConfiguracion();
    Task<ApiResponse<ConfiguracionSistema>> ActualizarConfiguracion(ConfiguracionSistemaDto dto);
}


using Microsoft.AspNetCore.Mvc;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;

namespace Facturno.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturacionController : ControllerBase
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IProfesionalRepository _profesionalRepository;
    private readonly IObraSocialRepository _obraSocialRepository;
    private readonly IFacturaPdfService _facturaPdfService;

    public FacturacionController(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IProfesionalRepository profesionalRepository,
        IObraSocialRepository obraSocialRepository,
        IFacturaPdfService facturaPdfService)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _profesionalRepository = profesionalRepository;
        _obraSocialRepository = obraSocialRepository;
        _facturaPdfService = facturaPdfService;
    }

    [HttpGet("comprobante/{idTurno:long}")]
    public async Task<IActionResult> DescargarComprobante(long idTurno)
    {
        var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno);
        if (turno == null)
        {
            return NotFound(ApiResponse<string>.Error("El turno especificado no existe."));
        }

        var paciente = await _pacienteRepository.ObtenerPorIdAsync(turno.IdPaciente);
        if (paciente == null)
        {
            return NotFound(ApiResponse<string>.Error("El paciente asociado al turno no existe."));
        }

        var profesional = await _profesionalRepository.ObtenerPorIdAsync(turno.IdProfesional);
        if (profesional == null)
        {
            return NotFound(ApiResponse<string>.Error("El profesional asociado al turno no existe."));
        }

        var obraSocial = await _obraSocialRepository.ObtenerPorIdAsync(paciente.IdObraSocial);

        var pdfBytes = _facturaPdfService.GenerarComprobanteTurnoPdf(turno, paciente, profesional, obraSocial);

        return File(pdfBytes, "application/pdf", $"comprobante_turno_{idTurno}.pdf");
    }
}

using Facturno.Shared.Models;

namespace Facturno.Shared.Interfaces;

public interface IFacturaPdfService
{
    byte[] GenerarComprobanteTurnoPdf(Turno turno, Paciente paciente, Profesional profesional, ObraSocial? obraSocial);
}

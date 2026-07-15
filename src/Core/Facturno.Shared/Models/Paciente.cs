namespace Facturno.Shared.Models;

public class Paciente : Usuario
{
    // Reemplazamos Dni por los nuevos campos del SQL
    public string NumDniCuil { get; set; } = string.Empty;
    public string TipoDniCuil { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public string NumObraSocial { get; set; } = string.Empty;

    public List<Turno> HistorialTurnos { get; set; } = new();
}
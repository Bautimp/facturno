namespace Facturno.Shared.Models;

public class Paciente : Usuario
{
    public string Dni { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;

    // Relación: Un paciente tiene muchos turnos
    public List<Turno> HistorialTurnos { get; set; } = new();
}
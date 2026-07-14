namespace Facturno.Shared.DTOs;

public class TurnoCreateDto
{
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public int IdPaciente { get; set; }
    public int IdProfesional { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
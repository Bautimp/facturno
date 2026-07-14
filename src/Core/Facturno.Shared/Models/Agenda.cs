namespace Facturno.Shared.Models;

public class Agenda
{
    public int IdAgenda { get; set; }
    
    public DateOnly Fecha { get; set; } 

    // Relación (Foreign Key)
    public int IdProfesional { get; set; }
    public Profesional? Profesional { get; set; }

    // Relación: Una agenda contiene muchos turnos
    public List<Turno> Turnos { get; set; } = new();
}
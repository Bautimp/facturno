namespace Facturno.Shared.Models;

public class Profesional : Usuario
{
    public string Matricula { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;

    // Relación: Un profesional administra una agenda
    public Agenda? AgendaPersonal { get; set; }
    
    // Relación auxiliar: Turnos asignados a este profesional
    public List<Turno> Turnos { get; set; } = new();
}
using Facturno.Shared.Enums;

namespace Facturno.Shared.Models;

public class Turno
{
    public int IdTurno { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; } // TimeOnly es ideal para horarios
    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;
    public string Observaciones { get; set; } = string.Empty;

    // Relación con Paciente
    public int IdPaciente { get; set; }
    public Paciente? Paciente { get; set; }

    // Relación con Profesional
    public int IdProfesional { get; set; }
    public Profesional? Profesional { get; set; }

    // Relación con Agenda
    public int? IdAgenda { get; set; }
    public Agenda? Agenda { get; set; }
}
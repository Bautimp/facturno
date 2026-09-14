using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Interfaces;

//Interfaz de repositorio para los turnos
public interface ITurnoRepository
{
    Task<Turno?> ObtenerPorIdAsync(long idTurno);
    Task<List<Turno>> ObtenerPorProfesionalYFechaAsync(Guid idProfesional, DateOnly fecha);
    Task<List<Turno>> ObtenerPorPacienteAsync(long idPaciente);
    Task<Turno> CrearAsync(Turno turno);
    Task<Turno> ActualizarAsync(Turno turno);
    Task<bool> EliminarAsync(long idTurno);
}

//Interfaz de repositorio para los pacientes
public interface IPacienteRepository
{
    Task<Paciente?> ObtenerPorIdAsync(long idPaciente);
    Task<List<Paciente>> ObtenerTodosAsync();
    Task<Paciente> CrearAsync(Paciente paciente, Persona persona);
    Task<Paciente> ActualizarAsync(Paciente paciente, Persona persona);
}

//Interfaz de repositorio para los profesionales
public interface IProfesionalRepository
{
    Task<Profesional?> ObtenerPorIdAsync(Guid idProfesional);
    Task<List<Profesional>> ObtenerTodosAsync();
    Task<Profesional> CrearAsync(Profesional profesional, Usuario usuario, Persona persona);
    Task<Profesional> ActualizarAsync(Profesional profesional, Persona persona);
}

//Interfaz de repositorio para los usuarios
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid idUsuario);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<bool> CambiarEstadoActivoAsync(Guid idUsuario, bool activo);
}

//Interfaz de repositorio para las obras sociales
public interface IObraSocialRepository
{
    Task<ObraSocial?> ObtenerPorIdAsync(long idObraSocial);
    Task<List<ObraSocial>> ObtenerTodasAsync();
    Task<ObraSocial> CrearAsync(ObraSocial obraSocial);
    Task<ObraSocial> ActualizarAsync(ObraSocial obraSocial);
}

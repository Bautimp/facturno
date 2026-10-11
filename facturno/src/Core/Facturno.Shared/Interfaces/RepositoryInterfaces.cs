using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Interfaces;

// Interfaz de repositorio para los turnos
public interface ITurnoRepository
{
    Task<Turno?> ObtenerPorId(long idTurno);
    Task<List<Turno>> ObtenerPorProfesionalYFecha(Guid idProfesional, DateOnly fecha);
    Task<List<Turno>> ObtenerPorPaciente(long idPaciente);
    Task<Turno> Crear(Turno turno);
    Task<Turno> Actualizar(Turno turno);
    Task<bool> Eliminar(long idTurno);
}

// Interfaz de repositorio para los pacientes
public interface IPacienteRepository
{
    Task<Paciente?> ObtenerPorId(long idPaciente);
    Task<List<Paciente>> ObtenerTodos();
    Task<Paciente> Crear(Paciente paciente, Persona persona);
    Task<Paciente> Actualizar(Paciente paciente, Persona persona);
}

// Interfaz de repositorio para los profesionales
public interface IProfesionalRepository
{
    Task<Profesional?> ObtenerPorId(Guid idProfesional);
    Task<List<Profesional>> ObtenerTodos();
    Task<Profesional> Crear(Profesional profesional, Usuario usuario, Persona persona);
    Task<Profesional> Actualizar(Profesional profesional, Persona persona);
}

// Interfaz de repositorio para los usuarios
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorId(Guid idUsuario);
    Task<Usuario?> ObtenerPorCorreo(string correo);
    Task<Usuario> CrearConPersona(Usuario usuario, Persona persona);
    Task<bool> CambiarEstadoActivo(Guid idUsuario, bool activo);
}

// Interfaz de repositorio para las obras sociales
public interface IObraSocialRepository
{
    Task<ObraSocial?> ObtenerPorId(long idObraSocial);
    Task<List<ObraSocial>> ObtenerTodas();
    Task<ObraSocial> Crear(ObraSocial obraSocial);
    Task<ObraSocial> Actualizar(ObraSocial obraSocial);
    Task<bool> CambiarEstadoActivo(long idObraSocial, bool activo);
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.API.Controllers;

[Authorize(Roles = "Operador")]
[ApiController]
[Route("api/[controller]")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalRepository _profesionalRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly global::Supabase.Client _supabaseClient;

    public ProfesionalesController(IProfesionalRepository profesionalRepository, IUsuarioRepository usuarioRepository, global::Supabase.Client supabaseClient)
    {
        _profesionalRepository = profesionalRepository;
        _usuarioRepository = usuarioRepository;
        _supabaseClient = supabaseClient;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Profesional>>>> ObtenerTodos()
    {
        var profesionales = await _profesionalRepository.ObtenerTodosAsync();
        return Ok(ApiResponse<List<Profesional>>.Ok(profesionales));
    }

    [HttpGet("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> ObtenerPorId(Guid idProfesional)
    {
        var profesional = await _profesionalRepository.ObtenerPorIdAsync(idProfesional);
        if (profesional == null)
        {
            return NotFound(ApiResponse<Profesional>.Error("Profesional no encontrado."));
        }
        return Ok(ApiResponse<Profesional>.Ok(profesional));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Profesional>>> Crear([FromBody] ProfesionalCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Profesional>.Error("Datos de entrada inválidos."));
        }

        try
        {
            // Verificar si el correo ya existe en nuestro sistema
            var usuarioExistente = !string.IsNullOrEmpty(dto.Correo) ? await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo) : null;
            if (usuarioExistente != null)
            {
                return BadRequest(ApiResponse<Profesional>.Error($"El correo '{dto.Correo}' ya se encuentra registrado para otro usuario."));
            }

            Guid nuevoIdUsuario;
            var passwordAUsar = !string.IsNullOrWhiteSpace(dto.Password) 
                ? dto.Password 
                : (!string.IsNullOrWhiteSpace(dto.Correo) ? dto.Correo + "123!" : "Facturno123!");

            // 1. Crear el usuario en auth.users de Supabase Auth
            try
            {
                var signupRes = await _supabaseClient.Auth.SignUp(dto.Correo, passwordAUsar);
                if (signupRes?.User != null && Guid.TryParse(signupRes.User.Id, out var parsedGuid))
                {
                    nuevoIdUsuario = parsedGuid;
                }
                else
                {
                    nuevoIdUsuario = Guid.NewGuid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Profesional>.Error($"No se pudo crear el acceso en Supabase Auth: {ex.Message}"));
            }

            var persona = new Persona
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Telefono = dto.Telefono
            };

            var usuario = new Usuario
            {
                IdUsuario = nuevoIdUsuario,
                Rol = RolUsuario.Profesional,
                Activo = true
            };

            var profesional = new Profesional
            {
                IdProfesional = nuevoIdUsuario,
                Especialidad = dto.Especialidad,
                Matricula = dto.Matricula,
                Cuit = dto.Cuit,
                PrecioConsulta = dto.PrecioConsulta,
                TipoComprobante = dto.TipoComprobante,
                CondicionIva = dto.CondicionIva
            };

            var creado = await _profesionalRepository.CrearAsync(profesional, usuario, persona);
            return CreatedAtAction(nameof(ObtenerPorId), new { idProfesional = creado.IdProfesional }, ApiResponse<Profesional>.Ok(creado, "Profesional creado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Profesional>.Error($"Error al guardar profesional: {ex.Message}"));
        }
    }

    [HttpPut("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> Actualizar(Guid idProfesional, [FromBody] ProfesionalUpdateDto dto)
    {
        try
        {
            var existente = await _profesionalRepository.ObtenerPorIdAsync(idProfesional);
            if (existente == null)
            {
                return NotFound(ApiResponse<Profesional>.Error("Profesional no encontrado."));
            }

            var persona = new Persona
            {
                IdPersona = existente.Usuario?.IdPersona ?? 0,
                Nombre = dto.Nombre ?? existente.Usuario?.Persona?.Nombre ?? string.Empty,
                Apellido = dto.Apellido ?? existente.Usuario?.Persona?.Apellido ?? string.Empty,
                Correo = dto.Correo ?? existente.Usuario?.Persona?.Correo,
                Telefono = dto.Telefono ?? existente.Usuario?.Persona?.Telefono
            };

            var profesional = new Profesional
            {
                IdProfesional = idProfesional,
                Especialidad = dto.Especialidad ?? existente.Especialidad,
                Matricula = dto.Matricula ?? existente.Matricula,
                Cuit = dto.Cuit ?? existente.Cuit,
                PrecioConsulta = dto.PrecioConsulta ?? existente.PrecioConsulta,
                TipoComprobante = dto.TipoComprobante ?? existente.TipoComprobante,
                CondicionIva = dto.CondicionIva ?? existente.CondicionIva
            };

            var actualizado = await _profesionalRepository.ActualizarAsync(profesional, persona);
            return Ok(ApiResponse<Profesional>.Ok(actualizado, "Profesional actualizado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Profesional>.Error($"Error al actualizar profesional: {ex.Message}"));
        }
    }

    [HttpPatch("{idProfesional:guid}/activo")]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoActivo(Guid idProfesional, [FromQuery] bool activo)
    {
        var resultado = await _usuarioRepository.CambiarEstadoActivoAsync(idProfesional, activo);
        if (!resultado)
        {
            return NotFound(ApiResponse<bool>.Error("No se pudo encontrar el usuario del profesional."));
        }

        var mensaje = activo ? "Profesional activado correctamente." : "Profesional desactivado correctamente. Turnos activos cancelados por sistema.";
        return Ok(ApiResponse<bool>.Ok(true, mensaje));
    }

    [HttpGet("administrativos")]
    public async Task<ActionResult<ApiResponse<List<Usuario>>>> ObtenerAdministrativos()
    {
        try
        {
            var usuariosRes = await _supabaseClient.From<UsuarioEntity>().Get();
            var personasRes = await _supabaseClient.From<PersonaEntity>().Get();
            var personasDict = personasRes.Models.ToDictionary(p => p.IdPersona);

            var adminList = usuariosRes.Models
                .Where(u => u.Rol == "Administrativo" || u.Rol == "Operador")
                .Select(u => new Usuario
                {
                    IdUsuario = Guid.TryParse(u.IdUsuario, out var g) ? g : Guid.Empty,
                    IdPersona = u.IdPersona,
                    Rol = Enum.TryParse<RolUsuario>(u.Rol, out var r) ? r : RolUsuario.Administrativo,
                    Activo = u.Activo,
                    Persona = personasDict.TryGetValue(u.IdPersona, out var p) ? new Persona
                    {
                        IdPersona = p.IdPersona,
                        Nombre = p.Nombre,
                        Apellido = p.Apellido,
                        Correo = p.Correo,
                        Telefono = p.Telefono
                    } : null
                })
                .ToList();

            return Ok(ApiResponse<List<Usuario>>.Ok(adminList));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Usuario>>.Error($"Error al obtener usuarios administrativos: {ex.Message}"));
        }
    }

    [HttpPost("vincular-agenda")]
    public async Task<ActionResult<ApiResponse<bool>>> VincularAgenda([FromQuery] Guid idProfesional, [FromQuery] string idAdministrativo)
    {
        try
        {
            var entity = new AgendaCompartidaEntity
            {
                IdAdministrativo = idAdministrativo,
                IdProfesional = idProfesional.ToString()
            };

            await _supabaseClient.From<AgendaCompartidaEntity>().Insert(entity);
            return Ok(ApiResponse<bool>.Ok(true, "Agenda vinculada correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Error($"Error o vinculación ya existente: {ex.Message}"));
        }
    }

    [HttpPost("administrativo")]
    public async Task<ActionResult<ApiResponse<Usuario>>> CrearAdministrativo([FromBody] AdministrativoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Usuario>.Error("Datos de entrada inválidos."));
        }

        try
        {
            var usuarioExistente = !string.IsNullOrEmpty(dto.Correo) ? await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo) : null;
            if (usuarioExistente != null)
            {
                return BadRequest(ApiResponse<Usuario>.Error($"El correo '{dto.Correo}' ya se encuentra registrado."));
            }

            Guid nuevoIdUsuario;
            var passwordAUsar = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : "Facturno123!";

            try
            {
                var signupRes = await _supabaseClient.Auth.SignUp(dto.Correo, passwordAUsar);
                if (signupRes?.User != null && Guid.TryParse(signupRes.User.Id, out var parsedGuid))
                {
                    nuevoIdUsuario = parsedGuid;
                }
                else
                {
                    nuevoIdUsuario = Guid.NewGuid();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Usuario>.Error($"No se pudo crear el acceso en Supabase Auth: {ex.Message}"));
            }

            var persona = new Persona
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Telefono = dto.Telefono
            };

            var usuario = new Usuario
            {
                IdUsuario = nuevoIdUsuario,
                Rol = RolUsuario.Administrativo,
                Activo = true
            };

            var creado = await _usuarioRepository.CrearConPersonaAsync(usuario, persona);
            return Ok(ApiResponse<Usuario>.Ok(creado, "Usuario Administrativo creado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Usuario>.Error($"Error al guardar usuario administrativo: {ex.Message}"));
        }
    }
}

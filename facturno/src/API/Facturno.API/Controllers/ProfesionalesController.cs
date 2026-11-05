using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.PocketBase;

namespace Facturno.API.Controllers;

[Authorize(Roles = "Profesional,Administrativo,Operador")]
[ApiController]
[Route("api/[controller]")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalRepository _profesionalRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly PocketBaseClient _pocketBaseClient;
    private readonly IConfiguracionService _configuracionService;

    public ProfesionalesController(
        IProfesionalRepository profesionalRepository, 
        IUsuarioRepository usuarioRepository, 
        PocketBaseClient pocketBaseClient,
        IConfiguracionService configuracionService)
    {
        _profesionalRepository = profesionalRepository;
        _usuarioRepository = usuarioRepository;
        _pocketBaseClient = pocketBaseClient;
        _configuracionService = configuracionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Profesional>>>> ObtenerTodos()
    {
        var profesionales = await _profesionalRepository.ObtenerTodos();
        return Ok(ApiResponse<List<Profesional>>.Ok(profesionales));
    }

    [HttpGet("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> ObtenerPorId(Guid idProfesional)
    {
        var profesional = await _profesionalRepository.ObtenerPorId(idProfesional);
        if (profesional == null)
        {
            return NotFound(ApiResponse<Profesional>.Error("Profesional no encontrado."));
        }
        return Ok(ApiResponse<Profesional>.Ok(profesional));
    }

    [Authorize(Roles = "Operador")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Profesional>>> Crear([FromBody] ProfesionalCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Profesional>.Error("Datos de entrada inválidos."));
        }

        var config = _configuracionService.ObtenerConfiguracion();
        if (dto.PrecioConsulta.HasValue && dto.PrecioConsulta.Value > config.PrecioMaximoConsulta)
        {
            return BadRequest(ApiResponse<Profesional>.Error($"El precio de la consulta ($ {dto.PrecioConsulta.Value:N2}) supera el precio máximo permitido en el consultorio ($ {config.PrecioMaximoConsulta:N2})."));
        }

        try
        {
            var usuarioExistente = !string.IsNullOrEmpty(dto.Correo) ? await _usuarioRepository.ObtenerPorCorreo(dto.Correo) : null;
            if (usuarioExistente != null)
            {
                return BadRequest(ApiResponse<Profesional>.Error($"El correo '{dto.Correo}' ya se encuentra registrado para otro usuario."));
            }

            var nuevoIdUsuario = Guid.NewGuid();
            var passwordAUsar = !string.IsNullOrWhiteSpace(dto.Password) 
                ? dto.Password 
                : (!string.IsNullOrWhiteSpace(dto.Correo) ? dto.Correo + "123!" : "Facturno123!");

            // 1. Resolver id_persona
            var todasPersonas = await _pocketBaseClient.GetList<PersonaPbRecord>("personas");
            long nuevoIdPersona = todasPersonas.Any() ? todasPersonas.Max(p => p.IdPersona) + 1 : 1;

            var personaCreada = await _pocketBaseClient.Create<PersonaPbRecord>("personas", new
            {
                id_persona = nuevoIdPersona,
                nombre = dto.Nombre,
                apellido = dto.Apellido,
                correo = dto.Correo,
                telefono = dto.Telefono
            });

            // 2. Crear usuario en PocketBase Auth
            var username = !string.IsNullOrWhiteSpace(dto.Correo) 
                ? dto.Correo.Split('@')[0].Replace(".", "_") 
                : $"user_{nuevoIdPersona}";

            await _pocketBaseClient.CreateUser(
                email: dto.Correo,
                password: passwordAUsar,
                username: username,
                rol: "Profesional",
                idPersona: nuevoIdPersona,
                idUsuario: nuevoIdUsuario.ToString()
            );

            // 3. Crear el profesional
            var persona = new Persona
            {
                IdPersona = nuevoIdPersona,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Telefono = dto.Telefono
            };

            var usuario = new Usuario
            {
                IdUsuario = nuevoIdUsuario,
                IdPersona = nuevoIdPersona,
                Rol = RolUsuario.Profesional,
                Activo = true,
                Persona = persona
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

            var creado = await _profesionalRepository.Crear(profesional, usuario, persona);
            return CreatedAtAction(nameof(ObtenerPorId), new { idProfesional = creado.IdProfesional }, ApiResponse<Profesional>.Ok(creado, "Profesional creado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Profesional>.Error($"Error al guardar profesional: {ex.Message}"));
        }
    }

    [HttpPut]
    [HttpPut("{idProfesional:guid}")]
    public async Task<ActionResult<ApiResponse<Profesional>>> Actualizar([FromBody] ProfesionalUpdateDto dto, [FromRoute] Guid? idProfesional = null)
    {
        try
        {
            var targetId = idProfesional ?? dto.IdProfesional;
            if (targetId == Guid.Empty)
            {
                return BadRequest(ApiResponse<Profesional>.Error("Identificador del profesional no especificado."));
            }

            var config = _configuracionService.ObtenerConfiguracion();
            if (dto.PrecioConsulta.HasValue && dto.PrecioConsulta.Value > config.PrecioMaximoConsulta)
            {
                return BadRequest(ApiResponse<Profesional>.Error($"El precio de la consulta ($ {dto.PrecioConsulta.Value:N2}) supera el precio máximo permitido en el consultorio ($ {config.PrecioMaximoConsulta:N2})."));
            }

            var existente = await _profesionalRepository.ObtenerPorId(targetId);
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
                IdProfesional = targetId,
                Especialidad = dto.Especialidad ?? existente.Especialidad,
                Matricula = dto.Matricula ?? existente.Matricula,
                Cuit = dto.Cuit ?? existente.Cuit,
                PrecioConsulta = dto.PrecioConsulta ?? existente.PrecioConsulta,
                TipoComprobante = dto.TipoComprobante ?? existente.TipoComprobante,
                CondicionIva = dto.CondicionIva ?? existente.CondicionIva
            };

            var actualizado = await _profesionalRepository.Actualizar(profesional, persona);
            return Ok(ApiResponse<Profesional>.Ok(actualizado, "Profesional actualizado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Profesional>.Error($"Error al actualizar profesional: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Operador")]
    [HttpPatch("{idProfesional:guid}/activo")]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoActivo(Guid idProfesional, [FromQuery] bool activo)
    {
        var resultado = await _usuarioRepository.CambiarEstadoActivo(idProfesional, activo);
        if (!resultado)
        {
            return NotFound(ApiResponse<bool>.Error("No se pudo encontrar el usuario del profesional."));
        }

        var mensaje = activo ? "Profesional activado correctamente." : "Profesional desactivado correctamente. Turnos activos cancelados por sistema.";
        return Ok(ApiResponse<bool>.Ok(true, mensaje));
    }

    [Authorize(Roles = "Operador,Administrativo,Profesional")]
    [HttpGet("administrativos")]
    public async Task<ActionResult<ApiResponse<List<Usuario>>>> ObtenerAdministrativos()
    {
        try
        {
            var usuarios = await _pocketBaseClient.GetList<PocketBaseUserRecord>("users");
            var personas = await _pocketBaseClient.GetList<PersonaPbRecord>("personas");
            var personasDict = personas.DistinctBy(p => p.IdPersona).ToDictionary(p => p.IdPersona);

            var adminList = usuarios
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

    [Authorize(Roles = "Operador,Profesional")]
    [HttpGet("agendas-compartidas")]
    public async Task<ActionResult<ApiResponse<List<AgendaCompartidaDetalleDto>>>> ObtenerAgendasCompartidas([FromQuery] Guid? idProfesional = null)
    {
        try
        {
            var esOperador = User.IsInRole("Operador");
            var esProfesional = User.IsInRole("Profesional");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdClaim, out var currentUserId);

            string filter = "";
            if (esProfesional && !esOperador)
            {
                filter = $"id_profesional='{currentUserId}'";
            }
            else if (idProfesional.HasValue && idProfesional.Value != Guid.Empty)
            {
                filter = $"id_profesional='{idProfesional.Value}'";
            }

            var vinculos = await _pocketBaseClient.GetList<AgendaCompartidaPbRecord>("agendas_compartidas", filter);
            var profesionales = await _profesionalRepository.ObtenerTodos();
            var usuarios = await _pocketBaseClient.GetList<PocketBaseUserRecord>("users");
            var personas = await _pocketBaseClient.GetList<PersonaPbRecord>("personas");
            var personasDict = personas.DistinctBy(p => p.IdPersona).ToDictionary(p => p.IdPersona);
            var usersDict = usuarios.ToDictionary(u => u.IdUsuario);
            var profDict = profesionales.ToDictionary(p => p.IdProfesional);

            var resultado = vinculos.Select(v =>
            {
                var profNom = "Profesional";
                if (Guid.TryParse(v.IdProfesional, out var pGuid) && profDict.TryGetValue(pGuid, out var prof))
                {
                    profNom = prof.Usuario?.Persona != null ? $"{prof.Usuario.Persona.Nombre} {prof.Usuario.Persona.Apellido}" : "Profesional";
                }

                var adminNom = "Administrativo";
                if (usersDict.TryGetValue(v.IdAdministrativo, out var uAdmin))
                {
                    if (personasDict.TryGetValue(uAdmin.IdPersona, out var pAdmin))
                    {
                        adminNom = $"{pAdmin.Nombre} {pAdmin.Apellido}";
                    }
                    else
                    {
                        adminNom = uAdmin.Email;
                    }
                }

                return new AgendaCompartidaDetalleDto
                {
                    Id = v.Id,
                    IdProfesional = Guid.TryParse(v.IdProfesional, out var pg) ? pg : Guid.Empty,
                    ProfesionalNombre = profNom,
                    IdAdministrativo = v.IdAdministrativo,
                    AdministrativoNombre = adminNom,
                    CreatedAt = v.Created
                };
            }).ToList();

            return Ok(ApiResponse<List<AgendaCompartidaDetalleDto>>.Ok(resultado));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<AgendaCompartidaDetalleDto>>.Error($"Error al listar agendas compartidas: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Operador,Profesional")]
    [HttpPost("vincular-agenda")]
    public async Task<ActionResult<ApiResponse<bool>>> VincularAgenda([FromQuery] Guid idProfesional, [FromQuery] string idAdministrativo)
    {
        try
        {
            var esOperador = User.IsInRole("Operador");
            if (!esOperador)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var currentUserId) || currentUserId != idProfesional)
                {
                    return Forbid();
                }
            }

            // Verificar si ya existe vínculo
            var existentes = await _pocketBaseClient.GetList<AgendaCompartidaPbRecord>(
                "agendas_compartidas", 
                $"id_profesional='{idProfesional}' && id_administrativo='{idAdministrativo}'");

            if (existentes.Any())
            {
                return BadRequest(ApiResponse<bool>.Error("La agenda ya se encuentra vinculada al administrativo seleccionado."));
            }

            var payload = new
            {
                id_administrativo = idAdministrativo,
                id_profesional = idProfesional.ToString()
            };

            await _pocketBaseClient.Create<AgendaCompartidaPbRecord>("agendas_compartidas", payload);
            return Ok(ApiResponse<bool>.Ok(true, "Agenda vinculada correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Error($"Error al vincular agenda: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Operador,Profesional")]
    [HttpDelete("desvincular-agenda")]
    public async Task<ActionResult<ApiResponse<bool>>> DesvincularAgenda([FromQuery] string? id = null, [FromQuery] Guid? idProfesional = null, [FromQuery] string? idAdministrativo = null)
    {
        try
        {
            var esOperador = User.IsInRole("Operador");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdClaim, out var currentUserId);

            AgendaCompartidaPbRecord? vinculo = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                vinculo = await _pocketBaseClient.GetById<AgendaCompartidaPbRecord>("agendas_compartidas", id);
            }
            else if (idProfesional.HasValue && !string.IsNullOrWhiteSpace(idAdministrativo))
            {
                vinculo = await _pocketBaseClient.GetFirst<AgendaCompartidaPbRecord>(
                    "agendas_compartidas", 
                    $"id_profesional='{idProfesional.Value}' && id_administrativo='{idAdministrativo}'");
            }

            if (vinculo == null)
            {
                return NotFound(ApiResponse<bool>.Error("Vínculo de agenda no encontrado."));
            }

            if (!esOperador)
            {
                if (!Guid.TryParse(vinculo.IdProfesional, out var vinculoProfId) || vinculoProfId != currentUserId)
                {
                    return Forbid();
                }
            }

            var deleted = await _pocketBaseClient.Delete("agendas_compartidas", vinculo.Id);
            if (!deleted)
            {
                return BadRequest(ApiResponse<bool>.Error("No se pudo desvincular la agenda."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Agenda desvinculada exitosamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Error($"Error al desvincular agenda: {ex.Message}"));
        }
    }

    [Authorize(Roles = "Operador")]
    [HttpPost("administrativo")]
    public async Task<ActionResult<ApiResponse<Usuario>>> CrearAdministrativo([FromBody] AdministrativoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<Usuario>.Error("Datos de entrada inválidos."));
        }

        try
        {
            var usuarioExistente = !string.IsNullOrEmpty(dto.Correo) ? await _usuarioRepository.ObtenerPorCorreo(dto.Correo) : null;
            if (usuarioExistente != null)
            {
                return BadRequest(ApiResponse<Usuario>.Error($"El correo '{dto.Correo}' ya se encuentra registrado."));
            }

            var todasPersonas = await _pocketBaseClient.GetList<PersonaPbRecord>("personas");
            long nuevoIdPersona = todasPersonas.Any() ? todasPersonas.Max(p => p.IdPersona) + 1 : 1;

            var nuevoIdUsuario = Guid.NewGuid();
            var passwordAUsar = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : "Facturno123!";
            var username = !string.IsNullOrWhiteSpace(dto.Correo) 
                ? dto.Correo.Split('@')[0].Replace(".", "_") 
                : $"admin_{nuevoIdPersona}";

            await _pocketBaseClient.Create<PersonaPbRecord>("personas", new
            {
                id_persona = nuevoIdPersona,
                nombre = dto.Nombre,
                apellido = dto.Apellido,
                correo = dto.Correo,
                telefono = dto.Telefono
            });

            await _pocketBaseClient.CreateUser(
                email: dto.Correo,
                password: passwordAUsar,
                username: username,
                rol: "Administrativo",
                idPersona: nuevoIdPersona,
                idUsuario: nuevoIdUsuario.ToString()
            );

            var persona = new Persona
            {
                IdPersona = nuevoIdPersona,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Telefono = dto.Telefono
            };

            var usuario = new Usuario
            {
                IdUsuario = nuevoIdUsuario,
                IdPersona = nuevoIdPersona,
                Rol = RolUsuario.Administrativo,
                Activo = true,
                Persona = persona
            };

            return Ok(ApiResponse<Usuario>.Ok(usuario, "Usuario Administrativo creado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Usuario>.Error($"Error al guardar usuario administrativo: {ex.Message}"));
        }
    }
}

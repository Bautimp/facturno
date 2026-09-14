using Microsoft.AspNetCore.Mvc;
using Facturno.Shared.Interfaces;
using Facturno.Shared.DTOs;

namespace Facturno.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly global::Supabase.Client _supabaseClient;

    public AuthController(IUsuarioRepository usuarioRepository, global::Supabase.Client supabaseClient)
    {
        _usuarioRepository = usuarioRepository;
        _supabaseClient = supabaseClient;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UsuarioAuthResponseDto>>> Login([FromBody] UsuarioLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(ApiResponse<UsuarioAuthResponseDto>.Error("Correo y contraseña son obligatorios."));
        }

        try
        {
            var session = await _supabaseClient.Auth.SignIn(dto.Correo, dto.Password);
            
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo);
            if (usuario == null || !usuario.Activo)
            {
                return Unauthorized(ApiResponse<UsuarioAuthResponseDto>.Error("Usuario inactivo o no registrado en la clínica."));
            }

            var nombreCompleto = usuario.Persona != null 
                ? $"{usuario.Persona.Nombre} {usuario.Persona.Apellido}" 
                : usuario.Persona?.Correo ?? dto.Correo;

            var authResponse = new UsuarioAuthResponseDto
            {
                IdUsuario = usuario.IdUsuario,
                Email = dto.Correo,
                NombreCompleto = nombreCompleto,
                Rol = usuario.Rol,
                TokenJwt = session?.AccessToken ?? string.Empty
            };

            return Ok(ApiResponse<UsuarioAuthResponseDto>.Ok(authResponse, "Inicio de sesión exitoso."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<UsuarioAuthResponseDto>.Error($"Error de autenticación: {ex.Message}"));
        }
    }
}

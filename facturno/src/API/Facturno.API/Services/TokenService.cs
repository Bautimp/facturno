using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Facturno.Shared.Models;

namespace Facturno.API.Services;

public interface ITokenService
{
    string GenerarToken(Usuario usuario);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarToken(Usuario usuario)
    {
        var keyStr = _configuration["Jwt:Key"] ?? "Facturno_Secret_Key_For_Jwt_Token_Generation_Must_Be_Long_Enough_2026";
        var issuer = _configuration["Jwt:Issuer"] ?? "Facturno.API";
        var audience = _configuration["Jwt:Audience"] ?? "Facturno.Clients";
        var expiresHoursStr = _configuration["Jwt:ExpiresInHours"] ?? "24";
        var expiresHours = double.TryParse(expiresHoursStr, out var h) ? h : 24;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var email = usuario.Persona?.Correo ?? $"{usuario.IdUsuario}@facturno.local";
        var rolStr = usuario.Rol.ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, rolStr),
            new("role", rolStr),
            new("id_usuario", usuario.IdUsuario.ToString()),
            new("id_persona", usuario.IdPersona.ToString())
        };

        if (usuario.Persona != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, $"{usuario.Persona.Nombre} {usuario.Persona.Apellido}".Trim()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiresHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

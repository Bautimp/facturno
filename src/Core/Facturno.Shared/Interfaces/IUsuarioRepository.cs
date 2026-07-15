using Facturno.Shared.Models;

namespace Facturno.Shared.Interfaces;

public interface IUsuarioRepository
{
    // Crucial para el inicio de sesión del sistema
    Task<Usuario?> ObtenerPorEmailAsync(string email); 
}
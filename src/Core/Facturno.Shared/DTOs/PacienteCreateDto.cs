namespace Facturno.Shared.DTOs;

public class PacienteCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    
    // Nuevos campos reflejados del SQL
    public string NumDniCuil { get; set; } = string.Empty;
    public string TipoDniCuil { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string ObraSocial { get; set; } = string.Empty;
    public string NumObraSocial { get; set; } = string.Empty;
}
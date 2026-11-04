using System.Text.RegularExpressions;

namespace Facturno.Shared.Utils;

public static class FormValidators
{
    // Correo electrónico: estándar RFC 5322 simplificado
    private static readonly Regex CorreoRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Documento: DNI (7-8 dígitos) o CUIL/Pasaporte (hasta 11 dígitos numéricos sin puntos)
    private static readonly Regex DocumentoRegex = new(
        @"^[0-9]{7,11}$",
        RegexOptions.Compiled);

    // Teléfono: 7 a 15 dígitos numéricos con posible prefijo '+' opcional
    private static readonly Regex TelefonoRegex = new(
        @"^\+?[0-9]{7,15}$",
        RegexOptions.Compiled);

    // Nombre de Obra Social: letras, números, espacios y signos habituales (2 a 60 caracteres)
    private static readonly Regex ObraSocialRegex = new(
        @"^[a-zA-Z0-9\sÁÉÍÓÚáéíóúñÑüÜ.\-_/()]{2,60}$",
        RegexOptions.Compiled);

    // Número de Afiliado / Obra Social: alfanumérico, guiones y barras (3 a 30 caracteres)
    private static readonly Regex NumObraSocialRegex = new(
        @"^[a-zA-Z0-9\-_/]{3,30}$",
        RegexOptions.Compiled);

    // CUIT: formato 20-12345678-9 o 11 dígitos continuos
    private static readonly Regex CuitRegex = new(
        @"^(\d{2}-\d{8}-\d{1}|\d{11})$",
        RegexOptions.Compiled);

    // Matrícula médica: letras, números y guiones (2 a 20 caracteres)
    private static readonly Regex MatriculaRegex = new(
        @"^[a-zA-Z0-9\-_]{2,20}$",
        RegexOptions.Compiled);

    public static bool EsCorreoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo)) return true; // Si es opcional
        return CorreoRegex.IsMatch(correo.Trim());
    }

    public static bool EsDocumentoValido(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento)) return true; // Si es opcional
        return DocumentoRegex.IsMatch(documento.Trim().Replace(".", "").Replace(" ", ""));
    }

    public static bool EsTelefonoValido(long? telefono)
    {
        if (!telefono.HasValue) return true;
        return TelefonoRegex.IsMatch(telefono.Value.ToString());
    }

    public static bool EsTelefonoValido(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return true;
        return TelefonoRegex.IsMatch(telefono.Trim().Replace(" ", "").Replace("-", ""));
    }

    public static bool EsObraSocialValida(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return false;
        return ObraSocialRegex.IsMatch(nombre.Trim());
    }

    public static bool EsNumObraSocialValido(string? numObraSocial)
    {
        if (string.IsNullOrWhiteSpace(numObraSocial)) return true;
        return NumObraSocialRegex.IsMatch(numObraSocial.Trim());
    }

    public static bool EsCuitValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit)) return true;
        return CuitRegex.IsMatch(cuit.Trim());
    }

    public static bool EsMatriculaValida(string? matricula)
    {
        if (string.IsNullOrWhiteSpace(matricula)) return true;
        return MatriculaRegex.IsMatch(matricula.Trim());
    }
}


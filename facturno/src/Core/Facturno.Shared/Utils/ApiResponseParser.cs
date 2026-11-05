using System.Text.Json;
using Facturno.Shared.DTOs;

namespace Facturno.Shared.Utils;

public static class ApiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<(bool Exito, string Mensaje, T? Datos)> ProcesarRespuestaAsync<T>(HttpResponseMessage response, string fallbackError = "Ocurrió un error al procesar la solicitud.")
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            return ParsearContenido<T>(raw, response.IsSuccessStatusCode, fallbackError, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return (false, $"{fallbackError}: {ex.Message}", default);
        }
    }

    public static (bool Exito, string Mensaje, T? Datos) ParsearContenido<T>(string? raw, bool isSuccessStatusCode, string fallbackError = "Ocurrió un error al procesar la solicitud.", int statusCode = 0)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            var defaultMsg = isSuccessStatusCode 
                ? "Operación realizada con éxito." 
                : (statusCode > 0 ? $"{fallbackError} (Código: {statusCode})" : fallbackError);
            return (isSuccessStatusCode, defaultMsg, default);
        }

        // 1. Intentar deserializar como ApiResponse<T>
        try
        {
            var apiResult = JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);
            if (apiResult != null && (!string.IsNullOrWhiteSpace(apiResult.Mensaje) || apiResult.Errores.Any() || apiResult.Exito))
            {
                var msg = !string.IsNullOrWhiteSpace(apiResult.Mensaje)
                    ? apiResult.Mensaje
                    : (apiResult.Errores.Any() ? string.Join(". ", apiResult.Errores) : fallbackError);

                return (apiResult.Exito, LimpiarMensaje(msg), apiResult.Datos);
            }
        }
        catch
        {
            // Continuar con inspectores de JSON alternativos
        }

        // 2. Intentar parsear estructuras JSON estándar (ProblemDetails, PocketBase, ASP.NET ModelState)
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                // Buscar si tiene diccionario de errores de validación (ASP.NET ModelState ProblemDetails)
                if (root.TryGetProperty("errors", out var pErrors) && pErrors.ValueKind == JsonValueKind.Object)
                {
                    var validationErrors = new List<string>();
                    foreach (var prop in pErrors.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                                    validationErrors.Add(item.GetString()!);
                            }
                        }
                        else if (prop.Value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(prop.Value.GetString()))
                        {
                            validationErrors.Add(prop.Value.GetString()!);
                        }
                    }

                    if (validationErrors.Any())
                    {
                        return (false, string.Join(". ", validationErrors), default);
                    }
                }

                if (root.TryGetProperty("mensaje", out var pMensaje) && pMensaje.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pMensaje.GetString()))
                {
                    return (false, LimpiarMensaje(pMensaje.GetString()!), default);
                }

                if (root.TryGetProperty("message", out var pMsg) && pMsg.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pMsg.GetString()))
                {
                    return (false, LimpiarMensaje(pMsg.GetString()!), default);
                }

                if (root.TryGetProperty("title", out var pTitle) && pTitle.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pTitle.GetString()))
                {
                    return (false, LimpiarMensaje(pTitle.GetString()!), default);
                }

                if (root.TryGetProperty("error", out var pErr) && pErr.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(pErr.GetString()))
                {
                    return (false, LimpiarMensaje(pErr.GetString()!), default);
                }
            }
        }
        catch
        {
            // No es JSON válido
        }

        // 3. Si no contiene llaves JSON ni etiquetas HTML, devolver el texto limpio si tiene un largo razonable
        var trimmed = raw.Trim();
        if (!trimmed.StartsWith("{") && !trimmed.StartsWith("<") && trimmed.Length < 300)
        {
            return (isSuccessStatusCode, trimmed, default);
        }

        return (isSuccessStatusCode, isSuccessStatusCode ? "Operación realizada con éxito." : fallbackError, default);
    }

    private static string LimpiarMensaje(string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje)) return string.Empty;
        var clean = mensaje.Trim();
        if (clean.StartsWith("{") && clean.EndsWith("}") && clean.Contains("\"mensaje\""))
        {
            try
            {
                using var doc = JsonDocument.Parse(clean);
                if (doc.RootElement.TryGetProperty("mensaje", out var m) && m.ValueKind == JsonValueKind.String)
                {
                    return m.GetString()?.Trim() ?? clean;
                }
            }
            catch { }
        }
        return clean;
    }
}


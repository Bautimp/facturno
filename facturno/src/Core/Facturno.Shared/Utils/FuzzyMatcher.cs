using System.Globalization;
using System.Text;
using Facturno.Shared.Models;

namespace Facturno.Shared.Utils;

public static class FuzzyMatcher
{
    /// <summary>
    /// Normaliza un texto convirtiéndolo a minúsculas y removiendo acentos / diacríticos.
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var normalizedString = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Calcula la distancia de Levenshtein entre dos cadenas.
    /// </summary>
    public static int DistanciaLevenshtein(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    /// <summary>
    /// Determina si un término de consulta coincide con una palabra objetivo,
    /// admitiendo prefijos, subcadenas y errores gramaticales/tipográficos leves (Levenshtein).
    /// </summary>
    public static bool CoincideTermino(string queryTerm, string targetWord)
    {
        if (string.IsNullOrEmpty(queryTerm) || string.IsNullOrEmpty(targetWord)) return false;

        // Coincidencia exacta o si una contiene a la otra
        if (targetWord.Contains(queryTerm) || queryTerm.Contains(targetWord))
            return true;

        // Si la palabra comienza con el término (autocompletado por prefijo)
        if (targetWord.StartsWith(queryTerm))
            return true;

        // Umbral de tolerancia según longitud del término buscado
        int maxDist;
        if (queryTerm.Length <= 3)
            maxDist = 0; // Palabras muy cortas (1-3 letras) requieren coincidencia exacta/prefijo
        else if (queryTerm.Length <= 6)
            maxDist = 1; // 4-6 letras: tolera 1 error (ej: matias vs matyas, perez vs peres)
        else
            maxDist = 2; // 7+ letras: tolera hasta 2 errores (ej: gonzales vs gonzalez, rodrigues vs rodriguez)

        // Distancia total
        int dist = DistanciaLevenshtein(queryTerm, targetWord);
        if (dist <= maxDist) return true;

        // Búsqueda en ventanas deslizantes para prefijos/sufijos largos
        if (targetWord.Length > queryTerm.Length)
        {
            int windowSize = queryTerm.Length;
            for (int i = 0; i <= targetWord.Length - windowSize; i++)
            {
                var sub = targetWord.Substring(i, windowSize);
                if (DistanciaLevenshtein(queryTerm, sub) <= Math.Min(1, maxDist))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Evalúa si los datos de un paciente coinciden con el término de búsqueda ingresado,
    /// buscando por nombre, apellido, combinaciones y DNI con tolerancia a errores ortográficos.
    /// </summary>
    public static bool EsCoincidenciaPaciente(Paciente paciente, string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return true;

        var cleanQuery = Normalizar(query);
        var queryTerms = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (queryTerms.Length == 0) return true;

        var nombre = Normalizar(paciente.Persona?.Nombre);
        var apellido = Normalizar(paciente.Persona?.Apellido);
        var dni = Normalizar(paciente.NumDocumento);

        var nombreCompleto = $"{nombre} {apellido}".Trim();
        var apellidoNombre = $"{apellido} {nombre}".Trim();

        // 1. Coincidencia directa completa en nombre completo o apellido
        if (nombreCompleto.Contains(cleanQuery) || apellidoNombre.Contains(cleanQuery))
            return true;

        // 2. Coincidencia directa en DNI
        if (!string.IsNullOrEmpty(dni) && (dni.Contains(cleanQuery) || cleanQuery.Contains(dni)))
            return true;

        // 3. Evaluar cada término por separado tolerando errores tipográficos
        var targetWords = $"{nombre} {apellido}".Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return queryTerms.All(qTerm =>
            targetWords.Any(tWord => CoincideTermino(qTerm, tWord)) ||
            (!string.IsNullOrEmpty(dni) && dni.Contains(qTerm)));
    }
}

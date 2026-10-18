using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Facturno.Infrastructure.PocketBase;

public class PocketBaseListResponse<T>
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();
}

public class PocketBaseAuthResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("record")]
    public PocketBaseUserRecord? Record { get; set; }
}

public class PocketBaseUserRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("rol")]
    public string Rol { get; set; } = string.Empty;

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    [JsonPropertyName("id_persona")]
    public long IdPersona { get; set; }

    [JsonPropertyName("id_usuario")]
    public string IdUsuario { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class PersonaPbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_persona")]
    public long IdPersona { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [JsonPropertyName("correo")]
    public string? Correo { get; set; }

    [JsonPropertyName("telefono")]
    public long? Telefono { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class AgendaCompartidaPbRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("id_administrativo")]
    public string IdAdministrativo { get; set; } = string.Empty;

    [JsonPropertyName("id_profesional")]
    public string IdProfesional { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
}

public class PocketBaseDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return DateTime.MinValue;
        return DateTime.TryParse(str, out var dt) ? dt : DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss.fffZ"));
    }
}

public class PocketBaseNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return null;
        return DateTime.TryParse(str, out var dt) ? dt : null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss.fffZ"));
        else
            writer.WriteNullValue();
    }
}

public class PocketBaseClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new PocketBaseDateTimeConverter());
        options.Converters.Add(new PocketBaseNullableDateTimeConverter());
        return options;
    }

    public PocketBaseClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = (configuration["PocketBase:BaseUrl"] ?? "http://127.0.0.1:8090").TrimEnd('/');
    }

    public async Task<List<T>> GetList<T>(string collection, string filter = "", string sort = "")
    {
        var url = $"{_baseUrl}/api/collections/{collection}/records?perPage=500";
        if (!string.IsNullOrWhiteSpace(filter))
        {
            url += $"&filter={Uri.EscapeDataString(filter)}";
        }
        if (!string.IsNullOrWhiteSpace(sort))
        {
            url += $"&sort={Uri.EscapeDataString(sort)}";
        }

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return new List<T>();
        }

        var result = await response.Content.ReadFromJsonAsync<PocketBaseListResponse<T>>(JsonOptions);
        return result?.Items ?? new List<T>();
    }

    public async Task<T?> GetFirst<T>(string collection, string filter)
    {
        var items = await GetList<T>(collection, filter);
        return items.FirstOrDefault();
    }

    public async Task<T?> GetById<T>(string collection, string id)
    {
        var url = $"{_baseUrl}/api/collections/{collection}/records/{id}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<T> Create<T>(string collection, object data)
    {
        var url = $"{_baseUrl}/api/collections/{collection}/records";
        var response = await _httpClient.PostAsJsonAsync(url, data, JsonOptions);
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear registro en PocketBase ({collection}): {response.StatusCode} - {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result!;
    }

    public async Task<T> Update<T>(string collection, string id, object data)
    {
        var url = $"{_baseUrl}/api/collections/{collection}/records/{id}";
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(data, options: JsonOptions)
        };

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al actualizar registro en PocketBase ({collection}/{id}): {response.StatusCode} - {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result!;
    }

    public async Task<bool> Delete(string collection, string id)
    {
        var url = $"{_baseUrl}/api/collections/{collection}/records/{id}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    public async Task<PocketBaseAuthResponse?> AuthenticateUser(string email, string password)
    {
        var url = $"{_baseUrl}/api/collections/users/auth-with-password";
        var payload = new { identity = email, password = password };
        var response = await _httpClient.PostAsJsonAsync(url, payload, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PocketBaseAuthResponse>(JsonOptions);
    }

    public async Task<PocketBaseUserRecord> CreateUser(string email, string password, string username, string rol, long idPersona, string idUsuario)
    {
        var url = $"{_baseUrl}/api/collections/users/records";
        var payload = new
        {
            email = email,
            password = password,
            passwordConfirm = password,
            username = username,
            rol = rol,
            activo = true,
            id_persona = idPersona,
            id_usuario = idUsuario
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al registrar usuario en PocketBase: {response.StatusCode} - {err}");
        }

        return (await response.Content.ReadFromJsonAsync<PocketBaseUserRecord>(JsonOptions))!;
    }
}

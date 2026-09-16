namespace Facturno.Shared.DTOs;

//Instanciar respuesta JSON de controladores y servicios

public class ApiResponse<T>
{
    public bool Exito { get; set; }
    public string? Mensaje { get; set; }
    public T? Datos { get; set; } //Datos del objeto devuelto (Turnos, Pacientes, Login, etc.)
    public List<string> Errores { get; set; } = new();

    // devuelve la respuesta OK con el objeto del turno agendado, objeto recibido o modificado
    public static ApiResponse<T> Ok(T datos, string? mensaje = null)
    {
        return new ApiResponse<T>
        {
            Exito = true,
            Datos = datos,
            Mensaje = mensaje
        };
    }

    // devuelve respuesta con error
    public static ApiResponse<T> Error(string error, List<string>? erroresAdicionales = null)
    {
        var response = new ApiResponse<T>
        {
            Exito = false,
            Mensaje = error
        };
        if (erroresAdicionales != null) response.Errores.AddRange(erroresAdicionales);
        return response;
    }
}

//instanciar sin datos
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? mensaje = null)
    {
        return new ApiResponse
        {
            Exito = true,
            Mensaje = mensaje
        };
    }
}

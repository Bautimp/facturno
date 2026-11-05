using System.Net.Http.Json;
using Facturno.Shared.DTOs;
using Facturno.Shared.Models;
using Facturno.Shared.Utils;
using Microsoft.AspNetCore.Components;

namespace Facturno.Blazor.Pages;

public partial class Configuracion : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected ConfiguracionSistemaDto Model = new();
    protected bool IsLoading = false;
    protected string? MensajeAlerta;
    protected string? MensajeExito;

    protected TimeOnly HoraInicioInput
    {
        get => TimeOnly.TryParse(Model.HoraInicioTurnos, out var t) ? t : new TimeOnly(8, 0);
        set => Model.HoraInicioTurnos = value.ToString("HH:mm");
    }

    protected TimeOnly HoraFinInput
    {
        get => TimeOnly.TryParse(Model.HoraFinTurnos, out var t) ? t : new TimeOnly(20, 0);
        set => Model.HoraFinTurnos = value.ToString("HH:mm");
    }

    protected override async Task OnInitializedAsync()
    {
        await CargarConfiguracion();
    }

    protected async Task CargarConfiguracion()
    {
        IsLoading = true;
        MensajeAlerta = null;
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<ConfiguracionSistema>>("api/configuracion");
            if (res != null && res.Exito && res.Datos != null)
            {
                Model = new ConfiguracionSistemaDto
                {
                    HoraInicioTurnos = res.Datos.HoraInicioTurnos,
                    HoraFinTurnos = res.Datos.HoraFinTurnos,
                    MaxPacientesPorDiaPorProfesional = res.Datos.MaxPacientesPorDiaPorProfesional,
                    PrecioMaximoConsulta = res.Datos.PrecioMaximoConsulta,
                    MinutosEntreTurnos = res.Datos.MinutosEntreTurnos
                };
            }
            else
            {
                MensajeAlerta = res?.Mensaje ?? "No se pudo cargar la configuración del sistema.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación al obtener configuración: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task GuardarConfiguracion()
    {
        // Validaciones en cliente
        if (!TimeOnly.TryParse(Model.HoraInicioTurnos, out var inicio) || !TimeOnly.TryParse(Model.HoraFinTurnos, out var fin))
        {
            MensajeAlerta = "El formato de los horarios debe ser válido (HH:mm).";
            return;
        }

        if (inicio >= fin)
        {
            MensajeAlerta = "La hora de inicio de turnos debe ser anterior a la hora de fin.";
            return;
        }

        if (Model.MaxPacientesPorDiaPorProfesional <= 0)
        {
            MensajeAlerta = "La cantidad máxima de pacientes por día debe ser al menos 1.";
            return;
        }

        if (Model.PrecioMaximoConsulta < 0)
        {
            MensajeAlerta = "El precio máximo de consulta no puede ser negativo.";
            return;
        }

        if (Model.MinutosEntreTurnos < 5 || Model.MinutosEntreTurnos > 240)
        {
            MensajeAlerta = "El intervalo entre turnos debe situarse entre 5 y 240 minutos.";
            return;
        }

        IsLoading = true;
        MensajeAlerta = null;
        MensajeExito = null;

        try
        {
            var response = await Http.PutAsJsonAsync("api/configuracion", Model);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<ConfiguracionSistema>(response, "Error al actualizar la configuración");

            if (exito && datos != null)
            {
                MensajeExito = mensaje ?? "Configuración del sistema y variables de entorno actualizadas con éxito.";
                Model = new ConfiguracionSistemaDto
                {
                    HoraInicioTurnos = datos.HoraInicioTurnos,
                    HoraFinTurnos = datos.HoraFinTurnos,
                    MaxPacientesPorDiaPorProfesional = datos.MaxPacientesPorDiaPorProfesional,
                    PrecioMaximoConsulta = datos.PrecioMaximoConsulta,
                    MinutosEntreTurnos = datos.MinutosEntreTurnos
                };
            }
            else
            {
                MensajeAlerta = mensaje;
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error inesperado al guardar la configuración: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void RestablecerPredeterminados()
    {
        Model.HoraInicioTurnos = "07:00";
        Model.HoraFinTurnos = "22:00";
        Model.MaxPacientesPorDiaPorProfesional = 10;
        Model.PrecioMaximoConsulta = 100000m;
        Model.MinutosEntreTurnos = 45;
        MensajeExito = "Valores predeterminados cargados en el formulario. Haz clic en 'Guardar Cambios' para aplicarlos.";
    }
}
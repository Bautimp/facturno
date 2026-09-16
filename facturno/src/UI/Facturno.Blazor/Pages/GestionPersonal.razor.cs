using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Blazor.Pages;

public partial class GestionPersonal : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected List<Profesional> ListaProfesionales = new();
    protected List<Usuario> ListaAdministrativos = new();
    protected string? MensajeAlerta;

    protected bool MostrarModal = false;
    protected bool MostrarModalAdmin = false;
    protected ProfesionalCreateDto profesionalDto = new();
    protected AdministrativoCreateDto adminDto = new();

    protected Guid IdProfesionalVinculo = Guid.Empty;
    protected string IdAdministrativoVinculoStr = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await CargarProfesionales();
        await CargarAdministrativos();
    }

    protected async Task CargarProfesionales()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Profesional>>>("api/profesionales");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaProfesionales = res.Datos;
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cargar profesionales: {ex.Message}";
        }
    }

    protected async Task CargarAdministrativos()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Usuario>>>("api/profesionales/administrativos");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaAdministrativos = res.Datos;
            }
        }
        catch
        {
            // Silencioso
        }
    }

    protected void AbrirModalNuevo()
    {
        profesionalDto = new ProfesionalCreateDto();
        MostrarModal = true;
    }

    protected async Task GuardarProfesional()
    {
        try
        {
            var res = await Http.PostAsJsonAsync("api/profesionales", profesionalDto);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Profesional>>();

            if (apiResult != null && apiResult.Exito)
            {
                MostrarModal = false;
                MensajeAlerta = "Profesional registrado correctamente.";
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo crear el profesional.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar profesional: {ex.Message}";
        }
    }

    protected void AbrirModalNuevoAdmin()
    {
        adminDto = new AdministrativoCreateDto();
        MostrarModalAdmin = true;
    }

    protected async Task GuardarAdministrativo()
    {
        try
        {
            var res = await Http.PostAsJsonAsync("api/profesionales/administrativo", adminDto);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Usuario>>();

            if (apiResult != null && apiResult.Exito)
            {
                MostrarModalAdmin = false;
                MensajeAlerta = "Usuario Administrativo registrado correctamente.";
                await CargarAdministrativos();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo crear el usuario administrativo.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar usuario administrativo: {ex.Message}";
        }
    }

    protected async Task ToggleEstadoActivo(Profesional prof, bool nuevoEstado)
    {
        try
        {
            var url = $"api/profesionales/{prof.IdProfesional}/activo?activo={nuevoEstado}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            var res = await Http.SendAsync(request);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (apiResult != null && apiResult.Exito)
            {
                MensajeAlerta = apiResult.Mensaje;
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "Error al cambiar el estado del profesional.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación: {ex.Message}";
        }
    }

    protected async Task VincularAgenda()
    {
        if (IdProfesionalVinculo == Guid.Empty || string.IsNullOrWhiteSpace(IdAdministrativoVinculoStr))
        {
            MensajeAlerta = "Debe seleccionar un profesional y un usuario administrativo.";
            return;
        }

        try
        {
            var url = $"api/profesionales/vincular-agenda?idProfesional={IdProfesionalVinculo}&idAdministrativo={IdAdministrativoVinculoStr}";
            var res = await Http.PostAsync(url, null);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (apiResult != null && apiResult.Exito)
            {
                MensajeAlerta = "Agenda del profesional vinculada correctamente al administrativo seleccionado.";
                IdProfesionalVinculo = Guid.Empty;
                IdAdministrativoVinculoStr = string.Empty;
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo vincular la agenda.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al vincular agenda: {ex.Message}";
        }
    }
}

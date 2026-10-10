using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Blazor.Pages;

public partial class GestionPersonal : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected string TabActiva = "personal";

    protected List<Profesional> ListaProfesionales = new();
    protected List<Usuario> ListaAdministrativos = new();
    protected List<ObraSocial> ListaObrasSociales = new();

    protected string? MensajeAlerta;

    protected bool MostrarModal = false;
    protected bool MostrarModalAdmin = false;
    protected bool MostrarModalEditar = false;
    protected ProfesionalCreateDto profesionalDto = new();
    protected ProfesionalUpdateDto profesionalEditarDto = new();
    protected AdministrativoCreateDto adminDto = new();

    // Estado Obras Sociales
    protected bool MostrarModalObraSocial = false;
    protected bool EsEdicionObraSocial = false;
    protected ObraSocial ObraSocialModelo = new();
    protected string FiltroObraSocial = string.Empty;

    protected IEnumerable<ObraSocial> ObrasSocialesFiltradas => string.IsNullOrWhiteSpace(FiltroObraSocial)
        ? ListaObrasSociales
        : ListaObrasSociales.Where(o => o.Nombre != null && o.Nombre.Contains(FiltroObraSocial, StringComparison.OrdinalIgnoreCase));

    protected Guid IdProfesionalVinculo = Guid.Empty;
    protected string IdAdministrativoVinculoStr = string.Empty;

    protected bool IsLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await CargarProfesionales();
        await CargarAdministrativos();
        await CargarObrasSociales();
    }

    protected async Task CargarProfesionales()
    {
        IsLoading = true;
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
        finally
        {
            IsLoading = false;
            StateHasChanged();
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

    protected void AbrirModalEditar(Profesional prof)
    {
        profesionalEditarDto = new ProfesionalUpdateDto
        {
            IdProfesional = prof.IdProfesional,
            Nombre = prof.Usuario?.Persona?.Nombre ?? string.Empty,
            Apellido = prof.Usuario?.Persona?.Apellido ?? string.Empty,
            Correo = prof.Usuario?.Persona?.Correo ?? string.Empty,
            Telefono = prof.Usuario?.Persona?.Telefono,
            Especialidad = prof.Especialidad,
            Matricula = prof.Matricula,
            Cuit = prof.Cuit,
            PrecioConsulta = prof.PrecioConsulta,
            TipoComprobante = prof.TipoComprobante,
            CondicionIva = prof.CondicionIva
        };
        MostrarModalEditar = true;
    }

    protected async Task ActualizarProfesional()
    {
        try
        {
            var res = await Http.PutAsJsonAsync($"api/profesionales/{profesionalEditarDto.IdProfesional}", profesionalEditarDto);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Profesional>>();

            if (apiResult != null && apiResult.Exito)
            {
                MostrarModalEditar = false;
                MensajeAlerta = $"Profesional {profesionalEditarDto.Nombre} {profesionalEditarDto.Apellido} actualizado correctamente.";
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo actualizar el profesional.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al actualizar profesional: {ex.Message}";
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

    // --- Métodos ABM Obras Sociales ---

    protected async Task CargarObrasSociales()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<ObraSocial>>>("api/obrassociales");
            if (res != null && res.Exito && res.Datos != null && res.Datos.Any())
            {
                ListaObrasSociales = res.Datos;
            }
            else
            {
                CargarObrasSocialesMock();
            }
        }
        catch
        {
            CargarObrasSocialesMock();
        }
    }

    private void CargarObrasSocialesMock()
    {
        if (!ListaObrasSociales.Any())
        {
            ListaObrasSociales = new List<ObraSocial>
            {
                new ObraSocial { IdObraSocial = 1, Nombre = "Particular", Activo = true },
                new ObraSocial { IdObraSocial = 2, Nombre = "OSDE", Activo = true },
                new ObraSocial { IdObraSocial = 3, Nombre = "Swiss Medical", Activo = true },
                new ObraSocial { IdObraSocial = 4, Nombre = "Galeno", Activo = true }
            };
        }
    }

    protected void AbrirModalNuevaObraSocial()
    {
        EsEdicionObraSocial = false;
        ObraSocialModelo = new ObraSocial { Activo = true };
        MostrarModalObraSocial = true;
    }

    protected void AbrirModalEditarObraSocial(ObraSocial os)
    {
        EsEdicionObraSocial = true;
        ObraSocialModelo = new ObraSocial
        {
            IdObraSocial = os.IdObraSocial,
            Nombre = os.Nombre,
            Activo = os.Activo,
            CreatedAt = os.CreatedAt
        };
        MostrarModalObraSocial = true;
    }

    protected async Task GuardarObraSocial()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ObraSocialModelo.Nombre))
            {
                MensajeAlerta = "El nombre de la obra social es obligatorio.";
                return;
            }

            HttpResponseMessage res;
            if (EsEdicionObraSocial)
            {
                res = await Http.PutAsJsonAsync($"api/obrassociales/{ObraSocialModelo.IdObraSocial}", ObraSocialModelo);
            }
            else
            {
                res = await Http.PostAsJsonAsync("api/obrassociales", ObraSocialModelo);
            }

            if (!res.IsSuccessStatusCode)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (EsEdicionObraSocial)
                    {
                        var target = ListaObrasSociales.FirstOrDefault(o => o.IdObraSocial == ObraSocialModelo.IdObraSocial);
                        if (target != null)
                        {
                            target.Nombre = ObraSocialModelo.Nombre;
                            target.Activo = ObraSocialModelo.Activo;
                        }
                    }
                    else
                    {
                        long nextId = ListaObrasSociales.Any() ? ListaObrasSociales.Max(o => o.IdObraSocial) + 1 : 1;
                        ListaObrasSociales.Add(new ObraSocial
                        {
                            IdObraSocial = nextId,
                            Nombre = ObraSocialModelo.Nombre,
                            Activo = ObraSocialModelo.Activo,
                            CreatedAt = DateTime.Now
                        });
                    }
                    MostrarModalObraSocial = false;
                    MensajeAlerta = EsEdicionObraSocial 
                        ? "Obra social actualizada." 
                        : "Obra social agregada correctamente. (Nota: Reiniciar Facturno.API en puerto 5082 para sincronizar con la base de datos).";
                    return;
                }

                var errStr = await res.Content.ReadAsStringAsync();
                MensajeAlerta = $"Error al guardar obra social ({res.StatusCode}): {(string.IsNullOrWhiteSpace(errStr) ? res.ReasonPhrase : errStr)}";
                return;
            }

            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<ObraSocial>>();
            if (apiResult != null && apiResult.Exito)
            {
                MostrarModalObraSocial = false;
                MensajeAlerta = EsEdicionObraSocial ? "Obra social actualizada con éxito." : "Obra social registrada con éxito.";
                await CargarObrasSociales();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "Error al guardar la obra social.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar obra social: {ex.Message}";
        }
    }

    protected async Task ToggleEstadoObraSocial(ObraSocial os, bool nuevoEstado)
    {
        try
        {
            var url = $"api/obrassociales/{os.IdObraSocial}/activo?activo={nuevoEstado}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            var res = await Http.SendAsync(request);

            if (!res.IsSuccessStatusCode)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    os.Activo = nuevoEstado;
                    MensajeAlerta = nuevoEstado ? "Obra Social activada." : "Obra Social dada de baja.";
                    return;
                }

                var errStr = await res.Content.ReadAsStringAsync();
                MensajeAlerta = $"Error al cambiar estado ({res.StatusCode}): {(string.IsNullOrWhiteSpace(errStr) ? res.ReasonPhrase : errStr)}";
                return;
            }

            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (apiResult != null && apiResult.Exito)
            {
                MensajeAlerta = apiResult.Mensaje;
                await CargarObrasSociales();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "Error al cambiar el estado de la obra social.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación: {ex.Message}";
        }
    }
}

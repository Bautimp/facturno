using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Utils;

namespace Facturno.Blazor.Pages;

public partial class GestionPersonal : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    protected bool EsOperador = false;
    protected bool EsProfesional = false;
    protected Guid IdProfesionalActual = Guid.Empty;

    protected string TabActiva = "personal";

    protected List<Profesional> ListaProfesionales = new();
    protected List<Usuario> ListaAdministrativos = new();
    protected List<ObraSocial> ListaObrasSociales = new();
    protected List<AgendaCompartidaDetalleDto> ListaVinculosAgendas = new();

    protected string? MensajeAlerta;

    protected bool MostrarModal = false;
    protected bool MostrarModalAdmin = false;
    protected bool MostrarModalEditar = false;
    protected ProfesionalCreateDto profesionalDto = new();
    protected ProfesionalUpdateDto profesionalEditarDto = new();
    protected AdministrativoCreateDto adminDto = new();

    // Paginación Personal Médico
    protected int PaginaPersonal = 1;
    protected int ElementosPorPaginaPersonal = 8;
    protected int TotalPaginasPersonal => Math.Max(1, (int)Math.Ceiling((double)ListaProfesionales.Count / ElementosPorPaginaPersonal));
    protected List<Profesional> ProfesionalesPaginados => ListaProfesionales
        .Skip((PaginaPersonal - 1) * ElementosPorPaginaPersonal)
        .Take(ElementosPorPaginaPersonal)
        .ToList();

    protected void CambiarPaginaPersonal(int nuevaPagina)
    {
        if (nuevaPagina >= 1 && nuevaPagina <= TotalPaginasPersonal)
        {
            PaginaPersonal = nuevaPagina;
        }
    }

    // Estado Obras Sociales
    protected bool MostrarModalObraSocial = false;
    protected bool EsEdicionObraSocial = false;
    protected ObraSocial ObraSocialModelo = new();
    protected string FiltroObraSocial = string.Empty;

    protected IEnumerable<ObraSocial> ObrasSocialesFiltradas => string.IsNullOrWhiteSpace(FiltroObraSocial)
        ? ListaObrasSociales
        : ListaObrasSociales.Where(o => o.Nombre != null && o.Nombre.Contains(FiltroObraSocial, StringComparison.OrdinalIgnoreCase));

    // Paginación Obras Sociales
    protected int PaginaObraSocial = 1;
    protected int ElementosPorPaginaObraSocial = 8;
    protected int TotalPaginasObraSocial => Math.Max(1, (int)Math.Ceiling((double)ObrasSocialesFiltradas.Count() / ElementosPorPaginaObraSocial));
    protected List<ObraSocial> ObrasSocialesPaginadas => ObrasSocialesFiltradas
        .Skip((PaginaObraSocial - 1) * ElementosPorPaginaObraSocial)
        .Take(ElementosPorPaginaObraSocial)
        .ToList();

    protected void OnFiltroObraSocialInput(ChangeEventArgs e)
    {
        FiltroObraSocial = e.Value?.ToString() ?? string.Empty;
        PaginaObraSocial = 1;
    }

    protected void CambiarPaginaObraSocial(int nuevaPagina)
    {
        if (nuevaPagina >= 1 && nuevaPagina <= TotalPaginasObraSocial)
        {
            PaginaObraSocial = nuevaPagina;
        }
    }

    protected Guid IdProfesionalVinculo = Guid.Empty;
    protected string IdAdministrativoVinculoStr = string.Empty;

    protected bool IsLoading = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                EsOperador = user.IsInRole("Operador");
                EsProfesional = user.IsInRole("Profesional");
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(idClaim, out var g))
                {
                    IdProfesionalActual = g;
                    if (EsProfesional && !EsOperador)
                    {
                        IdProfesionalVinculo = g;
                    }
                }
            }
        }
        catch
        {
            // Silencioso
        }

        await CargarProfesionales();
        await CargarAdministrativos();
        await CargarObrasSociales();
        await CargarVinculosAgendas();
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
                if (PaginaPersonal > TotalPaginasPersonal)
                {
                    PaginaPersonal = Math.Max(1, TotalPaginasPersonal);
                }
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
            if (string.IsNullOrWhiteSpace(profesionalDto.Nombre) || string.IsNullOrWhiteSpace(profesionalDto.Apellido))
            {
                MensajeAlerta = "El nombre y apellido del profesional son obligatorios.";
                return;
            }

            if (string.IsNullOrWhiteSpace(profesionalDto.Correo) || !FormValidators.EsCorreoValido(profesionalDto.Correo))
            {
                MensajeAlerta = "Debe ingresar un correo electrónico con formato válido (ej: medico@facturno.com).";
                return;
            }

            if (!FormValidators.EsTelefonoValido(profesionalDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                return;
            }

            if (!FormValidators.EsCuitValido(profesionalDto.Cuit))
            {
                MensajeAlerta = "El CUIT debe tener el formato 20-12345678-9 o 11 dígitos continuos.";
                return;
            }

            if (!FormValidators.EsMatriculaValida(profesionalDto.Matricula))
            {
                MensajeAlerta = "La matrícula profesional solo puede contener letras, números y guiones.";
                return;
            }

            if (string.IsNullOrWhiteSpace(profesionalDto.Password))
            {
                MensajeAlerta = "Debe asignar una contraseña inicial para el acceso al sistema.";
                return;
            }

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
            if (string.IsNullOrWhiteSpace(profesionalEditarDto.Nombre) || string.IsNullOrWhiteSpace(profesionalEditarDto.Apellido))
            {
                MensajeAlerta = "El nombre y apellido son obligatorios.";
                return;
            }

            if (!FormValidators.EsCorreoValido(profesionalEditarDto.Correo))
            {
                MensajeAlerta = "El correo electrónico no tiene un formato válido.";
                return;
            }

            if (!FormValidators.EsTelefonoValido(profesionalEditarDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                return;
            }

            if (!FormValidators.EsCuitValido(profesionalEditarDto.Cuit))
            {
                MensajeAlerta = "El CUIT debe tener el formato 20-12345678-9 o 11 dígitos continuos.";
                return;
            }

            if (!FormValidators.EsMatriculaValida(profesionalEditarDto.Matricula))
            {
                MensajeAlerta = "La matrícula profesional solo puede contener letras, números y guiones.";
                return;
            }

            var res = await Http.PutAsJsonAsync($"api/profesionales/{profesionalEditarDto.IdProfesional}", profesionalEditarDto);
            if (!res.IsSuccessStatusCode)
            {
                var errStr = await res.Content.ReadAsStringAsync();
                MensajeAlerta = $"Error al actualizar profesional: {errStr}";
                return;
            }

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
            if (string.IsNullOrWhiteSpace(adminDto.Nombre) || string.IsNullOrWhiteSpace(adminDto.Apellido))
            {
                MensajeAlerta = "El nombre y apellido son obligatorios.";
                return;
            }

            if (string.IsNullOrWhiteSpace(adminDto.Correo) || !FormValidators.EsCorreoValido(adminDto.Correo))
            {
                MensajeAlerta = "Debe ingresar un correo electrónico válido.";
                return;
            }

            if (!FormValidators.EsTelefonoValido(adminDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                return;
            }

            if (string.IsNullOrWhiteSpace(adminDto.Password))
            {
                MensajeAlerta = "Debe asignar una contraseña para el administrativo.";
                return;
            }

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

    protected async Task CargarVinculosAgendas()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<AgendaCompartidaDetalleDto>>>("api/profesionales/agendas-compartidas");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaVinculosAgendas = res.Datos;
            }
        }
        catch
        {
            // Silencioso
        }
    }

    protected async Task DesvincularAgenda(string idVinculo)
    {
        try
        {
            var url = $"api/profesionales/desvincular-agenda?id={idVinculo}";
            var res = await Http.DeleteAsync(url);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (apiResult != null && apiResult.Exito)
            {
                MensajeAlerta = "Agenda desvinculada exitosamente.";
                await CargarVinculosAgendas();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo desvincular la agenda.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al desvincular agenda: {ex.Message}";
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
                if (!EsProfesional || EsOperador)
                {
                    IdProfesionalVinculo = Guid.Empty;
                }
                IdAdministrativoVinculoStr = string.Empty;
                await CargarVinculosAgendas();
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
                if (PaginaObraSocial > TotalPaginasObraSocial)
                {
                    PaginaObraSocial = Math.Max(1, TotalPaginasObraSocial);
                }
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
            if (!FormValidators.EsObraSocialValida(ObraSocialModelo.Nombre))
            {
                MensajeAlerta = "El nombre de la obra social debe tener entre 2 y 60 caracteres alfanuméricos válidos.";
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
                        : "Obra social agregada correctamente.";
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

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
    protected ConfiguracionSistema ConfigSistema = new();

    protected string TabActiva = "personal";

    protected List<Profesional> ListaProfesionales = new();
    protected List<Usuario> ListaAdministrativos = new();
    protected List<ObraSocial> ListaObrasSociales = new();
    protected List<AgendaCompartidaDetalleDto> ListaVinculosAgendas = new();

    protected string? MensajeAlerta;
    protected string? TipoAlerta;

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

        await CargarConfiguracion();
        await CargarProfesionales();
        await CargarAdministrativos();
        await CargarObrasSociales();
        await CargarVinculosAgendas();
    }

    private async Task CargarConfiguracion()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<ConfiguracionSistema>>("api/configuracion");
            if (res != null && res.Exito && res.Datos != null)
            {
                ConfigSistema = res.Datos;
            }
        }
        catch
        {
            // Silencioso
        }
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

                if (EsProfesional && !EsOperador && IdProfesionalActual != Guid.Empty)
                {
                    var miPerfil = ListaProfesionales.FirstOrDefault(p => p.IdProfesional == IdProfesionalActual);
                    if (miPerfil != null)
                    {
                        profesionalEditarDto = new ProfesionalUpdateDto
                        {
                            IdProfesional = miPerfil.IdProfesional,
                            Nombre = miPerfil.Usuario?.Persona?.Nombre ?? string.Empty,
                            Apellido = miPerfil.Usuario?.Persona?.Apellido ?? string.Empty,
                            Correo = miPerfil.Usuario?.Persona?.Correo ?? string.Empty,
                            Telefono = miPerfil.Usuario?.Persona?.Telefono,
                            Especialidad = miPerfil.Especialidad,
                            Matricula = miPerfil.Matricula,
                            Cuit = miPerfil.Cuit,
                            PrecioConsulta = miPerfil.PrecioConsulta,
                            TipoComprobante = miPerfil.TipoComprobante,
                            CondicionIva = miPerfil.CondicionIva
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cargar profesionales: {ex.Message}";
            TipoAlerta = "danger";
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
            // Validaciones (Controles del sistema -> Warning)
            if (string.IsNullOrWhiteSpace(profesionalDto.Nombre) || string.IsNullOrWhiteSpace(profesionalDto.Apellido))
            {
                MensajeAlerta = "El nombre y apellido del profesional son obligatorios.";
                TipoAlerta = "warning";
                return;
            }

            if (string.IsNullOrWhiteSpace(profesionalDto.Correo) || !FormValidators.EsCorreoValido(profesionalDto.Correo))
            {
                MensajeAlerta = "Debe ingresar un correo electrónico con formato válido (ej: medico@facturno.com).";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsTelefonoValido(profesionalDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsCuitValido(profesionalDto.Cuit))
            {
                MensajeAlerta = "El CUIT debe tener el formato 20-12345678-9 o 11 dígitos continuos.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsMatriculaValida(profesionalDto.Matricula))
            {
                MensajeAlerta = "La matrícula profesional solo puede contener letras, números y guiones.";
                TipoAlerta = "warning";
                return;
            }

            if (string.IsNullOrWhiteSpace(profesionalDto.Password))
            {
                MensajeAlerta = "Debe asignar una contraseña inicial para el acceso al sistema.";
                TipoAlerta = "warning";
                return;
            }

            if (profesionalDto.PrecioConsulta.HasValue && profesionalDto.PrecioConsulta.Value > ConfigSistema.PrecioMaximoConsulta)
            {
                MensajeAlerta = $"El precio de consulta ($ {profesionalDto.PrecioConsulta.Value:N2}) supera el máximo permitido en el consultorio ($ {ConfigSistema.PrecioMaximoConsulta:N2}).";
                TipoAlerta = "warning";
                return;
            }

            var res = await Http.PostAsJsonAsync("api/profesionales", profesionalDto);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<Profesional>(res, "No se pudo crear el profesional.");

            if (exito)
            {
                MostrarModal = false;
                MensajeAlerta = "Profesional registrado correctamente.";
                TipoAlerta = "success";
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar profesional: {ex.Message}";
            TipoAlerta = "danger";
        }
    }

    protected void AbrirModalEditar(Profesional prof)
    {
        if (!EsOperador && prof.IdProfesional != IdProfesionalActual)
        {
            MensajeAlerta = "No tienes permisos para modificar datos de otros profesionales.";
            TipoAlerta = "warning";
            return;
        }

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
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsCorreoValido(profesionalEditarDto.Correo))
            {
                MensajeAlerta = "El correo electrónico no tiene un formato válido.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsTelefonoValido(profesionalEditarDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsCuitValido(profesionalEditarDto.Cuit))
            {
                MensajeAlerta = "El CUIT debe tener el formato 20-12345678-9 o 11 dígitos continuos.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsMatriculaValida(profesionalEditarDto.Matricula))
            {
                MensajeAlerta = "La matrícula profesional solo puede contener letras, números y guiones.";
                TipoAlerta = "warning";
                return;
            }

            if (profesionalEditarDto.PrecioConsulta.HasValue && profesionalEditarDto.PrecioConsulta.Value > ConfigSistema.PrecioMaximoConsulta)
            {
                MensajeAlerta = $"El precio de consulta ($ {profesionalEditarDto.PrecioConsulta.Value:N2}) supera el máximo permitido en el consultorio ($ {ConfigSistema.PrecioMaximoConsulta:N2}).";
                TipoAlerta = "warning";
                return;
            }

            var res = await Http.PutAsJsonAsync($"api/profesionales/{profesionalEditarDto.IdProfesional}", profesionalEditarDto);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<Profesional>(res, "No se pudo actualizar el profesional.");

            if (exito)
            {
                MostrarModalEditar = false;
                MensajeAlerta = EsProfesional && !EsOperador
                    ? "Tus datos personales han sido actualizados correctamente."
                    : $"Profesional {profesionalEditarDto.Nombre} {profesionalEditarDto.Apellido} actualizado correctamente.";
                TipoAlerta = "success";
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al actualizar profesional: {ex.Message}";
            TipoAlerta = "danger";
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
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsCorreoValido(adminDto.Correo))
            {
                MensajeAlerta = "Debe ingresar un correo electrónico válido.";
                TipoAlerta = "warning";
                return;
            }

            if (!FormValidators.EsTelefonoValido(adminDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                TipoAlerta = "warning";
                return;
            }

            if (string.IsNullOrWhiteSpace(adminDto.Password))
            {
                MensajeAlerta = "Debe asignar una contraseña para el administrativo.";
                TipoAlerta = "warning";
                return;
            }

            var res = await Http.PostAsJsonAsync("api/profesionales/administrativo", adminDto);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<Usuario>(res, "No se pudo crear el usuario administrativo.");

            if (exito)
            {
                MostrarModalAdmin = false;
                MensajeAlerta = "Usuario Administrativo registrado correctamente.";
                TipoAlerta = "success";
                await CargarAdministrativos();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar usuario administrativo: {ex.Message}";
            TipoAlerta = "danger";
        }
    }

    protected async Task ToggleEstadoActivo(Profesional prof, bool nuevoEstado)
    {
        if (!EsOperador)
        {
            MensajeAlerta = "Solo los usuarios con rol Operador pueden dar de alta o baja al personal médico.";
            TipoAlerta = "warning";
            return;
        }

        try
        {
            var url = $"api/profesionales/{prof.IdProfesional}/activo?activo={nuevoEstado}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            var res = await Http.SendAsync(request);
            var (exito, mensaje, _) = await ApiResponseParser.ProcesarRespuestaAsync<bool>(res, "Error al cambiar el estado del profesional.");

            if (exito)
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "success";
                await CargarProfesionales();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación: {ex.Message}";
            TipoAlerta = "danger";
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
            var (exito, mensaje, _) = await ApiResponseParser.ProcesarRespuestaAsync<bool>(res, "No se pudo desvincular la agenda.");

            if (exito)
            {
                MensajeAlerta = "Agenda desvinculada exitosamente.";
                TipoAlerta = "success";
                await CargarVinculosAgendas();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al desvincular agenda: {ex.Message}";
            TipoAlerta = "danger";
        }
    }

    protected async Task VincularAgenda()
    {
        if (IdProfesionalVinculo == Guid.Empty || string.IsNullOrWhiteSpace(IdAdministrativoVinculoStr))
        {
            MensajeAlerta = "Debe seleccionar un profesional y un usuario administrativo.";
            TipoAlerta = "warning";
            return;
        }

        try
        {
            var url = $"api/profesionales/vincular-agenda?idProfesional={IdProfesionalVinculo}&idAdministrativo={IdAdministrativoVinculoStr}";
            var res = await Http.PostAsync(url, null);
            var (exito, mensaje, _) = await ApiResponseParser.ProcesarRespuestaAsync<bool>(res, "No se pudo vincular la agenda.");

            if (exito)
            {
                MensajeAlerta = "Agenda del profesional vinculada correctamente al administrativo seleccionado.";
                TipoAlerta = "success";
                if (!EsProfesional || EsOperador)
                {
                    IdProfesionalVinculo = Guid.Empty;
                }
                IdAdministrativoVinculoStr = string.Empty;
                await CargarVinculosAgendas();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al vincular agenda: {ex.Message}";
            TipoAlerta = "danger";
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
                TipoAlerta = "warning";
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
                    TipoAlerta = "success";
                    return;
                }
            }

            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<ObraSocial>(res, "Error al guardar la obra social.");
            if (exito)
            {
                MostrarModalObraSocial = false;
                MensajeAlerta = EsEdicionObraSocial ? "Obra social actualizada con éxito." : "Obra social registrada con éxito.";
                TipoAlerta = "success";
                await CargarObrasSociales();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar obra social: {ex.Message}";
            TipoAlerta = "danger";
        }
    }

    protected async Task ToggleEstadoObraSocial(ObraSocial os, bool nuevoEstado)
    {
        try
        {
            var url = $"api/obrassociales/{os.IdObraSocial}/activo?activo={nuevoEstado}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            var res = await Http.SendAsync(request);

            if (!res.IsSuccessStatusCode && res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                os.Activo = nuevoEstado;
                MensajeAlerta = nuevoEstado ? "Obra Social activada." : "Obra Social dada de baja.";
                TipoAlerta = "success";
                return;
            }

            var (exito, mensaje, _) = await ApiResponseParser.ProcesarRespuestaAsync<bool>(res, "Error al cambiar el estado de la obra social.");
            if (exito)
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "success";
                await CargarObrasSociales();
            }
            else
            {
                MensajeAlerta = mensaje;
                TipoAlerta = "warning";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación: {ex.Message}";
            TipoAlerta = "danger";
        }
    }
}

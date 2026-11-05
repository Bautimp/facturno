using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Utils;

namespace Facturno.Blazor.Pages;

public partial class Pacientes : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected List<Paciente> ListaPacientes = new();
    protected List<ObraSocial> ListaObrasSociales = new();
    protected string FiltroBusqueda = string.Empty;
    protected string FiltroEstado = "todos"; // "todos", "activos", "inactivos"
    protected string? MensajeAlerta;

    protected bool MostrarModal = false;
    protected bool EsEdicion = false;
    protected PacienteCreateDto pacienteDto = new();

    // Paginación
    protected int PaginaActual = 1;
    protected int ElementosPorPagina = 8;

    protected List<Paciente> PacientesFiltradosLista
    {
        get
        {
            var query = ListaPacientes.AsEnumerable();

            if (FiltroEstado == "activos")
            {
                query = query.Where(p => p.Activo);
            }
            else if (FiltroEstado == "inactivos")
            {
                query = query.Where(p => !p.Activo);
            }

            if (!string.IsNullOrWhiteSpace(FiltroBusqueda))
            {
                query = query.Where(p => FuzzyMatcher.EsCoincidenciaPaciente(p, FiltroBusqueda));
            }

            return query.ToList();
        }
    }

    protected int TotalPaginas => Math.Max(1, (int)Math.Ceiling((double)PacientesFiltradosLista.Count / ElementosPorPagina));

    protected List<Paciente> PacientesPaginados => PacientesFiltradosLista
        .Skip((PaginaActual - 1) * ElementosPorPagina)
        .Take(ElementosPorPagina)
        .ToList();

    protected bool EsObraSocialParticular =>
        pacienteDto.IdObraSocial == 1 ||
        ListaObrasSociales.FirstOrDefault(os => os.IdObraSocial == pacienteDto.IdObraSocial)?.Nombre.Equals("Particular", StringComparison.OrdinalIgnoreCase) == true;

    protected bool IsLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await CargarObrasSociales();
        await CargarPacientes();
    }

    protected void OnFiltroBusquedaInput(ChangeEventArgs e)
    {
        FiltroBusqueda = e.Value?.ToString() ?? string.Empty;
        PaginaActual = 1;
    }

    protected void OnFiltroEstadoChanged(ChangeEventArgs e)
    {
        FiltroEstado = e.Value?.ToString() ?? "todos";
        PaginaActual = 1;
    }

    protected void OnNumDocumentoInput(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? string.Empty;
        pacienteDto.NumDocumento = new string(raw.Where(char.IsDigit).Take(11).ToArray());
    }

    protected void CambiarPagina(int nuevaPagina)
    {
        if (nuevaPagina >= 1 && nuevaPagina <= TotalPaginas)
        {
            PaginaActual = nuevaPagina;
        }
    }

    protected async Task CargarPacientes()
    {
        IsLoading = true;
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Paciente>>>("api/pacientes");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaPacientes = res.Datos;
                if (PaginaActual > TotalPaginas)
                {
                    PaginaActual = Math.Max(1, TotalPaginas);
                }
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cargar pacientes: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task CargarObrasSociales()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<ObraSocial>>>("api/obrassociales");
            if (res != null && res.Exito && res.Datos != null && res.Datos.Any())
            {
                ListaObrasSociales = res.Datos.Where(os => os.Activo).ToList();
            }
            else
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
        catch
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

    protected void AbrirModalNuevo()
    {
        EsEdicion = false;
        pacienteDto = new PacienteCreateDto
        {
            IdObraSocial = 1 // Particular por defecto
        };
        MostrarModal = true;
    }

    protected void OnObraSocialChanged(ChangeEventArgs e)
    {
        if (long.TryParse(e.Value?.ToString(), out var id))
        {
            pacienteDto.IdObraSocial = id;
            if (EsObraSocialParticular)
            {
                pacienteDto.NumObraSocial = null;
            }
        }
    }

    protected void EditarPaciente(Paciente paciente)
    {
        EsEdicion = true;
        pacienteDto = new PacienteUpdateDto
        {
            IdPaciente = paciente.IdPaciente,
            Nombre = paciente.Persona?.Nombre ?? string.Empty,
            Apellido = paciente.Persona?.Apellido ?? string.Empty,
            Correo = paciente.Persona?.Correo,
            Telefono = paciente.Persona?.Telefono,
            TipoDocumento = paciente.TipoDocumento,
            NumDocumento = paciente.NumDocumento,
            IdObraSocial = paciente.IdObraSocial,
            NumObraSocial = paciente.NumObraSocial,
            PorcentajeIva = paciente.PorcentajeIva,
            Activo = paciente.Activo
        };
        MostrarModal = true;
    }

    protected async Task ToggleEstadoActivo(Paciente pac, bool nuevoEstado)
    {
        try
        {
            var url = $"api/pacientes/{pac.IdPaciente}/activo?activo={nuevoEstado}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            var res = await Http.SendAsync(request);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (apiResult != null && apiResult.Exito)
            {
                MensajeAlerta = apiResult.Mensaje;
                await CargarPacientes();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "Error al cambiar el estado del paciente.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error de comunicación: {ex.Message}";
        }
    }

    protected async Task GuardarPaciente()
    {
        try
        {
            // Validaciones con Expresiones Regulares
            if (string.IsNullOrWhiteSpace(pacienteDto.Nombre) || string.IsNullOrWhiteSpace(pacienteDto.Apellido))
            {
                MensajeAlerta = "El nombre y apellido son obligatorios.";
                return;
            }

            if (!FormValidators.EsCorreoValido(pacienteDto.Correo))
            {
                MensajeAlerta = "El correo electrónico no tiene un formato válido (ej: usuario@correo.com).";
                return;
            }

            if (!FormValidators.EsTelefonoValido(pacienteDto.Telefono))
            {
                MensajeAlerta = "El teléfono debe contener entre 7 y 15 dígitos numéricos.";
                return;
            }

            if (!FormValidators.EsDocumentoValido(pacienteDto.NumDocumento))
            {
                MensajeAlerta = "El número de documento debe contener entre 7 y 11 dígitos numéricos sin puntos ni espacios.";
                return;
            }

            // Regla: Solo Particular puede tener Número de Obra Social nulo
            if (EsObraSocialParticular)
            {
                pacienteDto.NumObraSocial = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(pacienteDto.NumObraSocial))
                {
                    MensajeAlerta = "El número de afiliado de obra social es obligatorio cuando la cobertura no es 'Particular'.";
                    return;
                }

                if (!FormValidators.EsNumObraSocialValido(pacienteDto.NumObraSocial))
                {
                    MensajeAlerta = "El número de obra social solo puede contener caracteres alfanuméricos, guiones y barras (3 a 30 caracteres).";
                    return;
                }
            }

            HttpResponseMessage res;
            if (EsEdicion)
            {
                res = await Http.PutAsJsonAsync("api/pacientes", pacienteDto);
            }
            else
            {
                res = await Http.PostAsJsonAsync("api/pacientes", pacienteDto);
            }

            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Paciente>>();

            if (apiResult != null && apiResult.Exito)
            {
                MostrarModal = false;
                MensajeAlerta = EsEdicion ? "Paciente actualizado con éxito." : "Paciente registrado con éxito.";
                await CargarPacientes();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo guardar el paciente.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar paciente: {ex.Message}";
        }
    }
}

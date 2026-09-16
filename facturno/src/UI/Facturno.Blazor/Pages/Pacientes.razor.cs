using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;

namespace Facturno.Blazor.Pages;

public partial class Pacientes : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected List<Paciente> ListaPacientes = new();
    protected List<ObraSocial> ListaObrasSociales = new();
    protected string FiltroBusqueda = string.Empty;
    protected string? MensajeAlerta;

    protected bool MostrarModal = false;
    protected bool EsEdicion = false;
    protected PacienteCreateDto pacienteDto = new();

    protected IEnumerable<Paciente> PacientesFiltrados => string.IsNullOrWhiteSpace(FiltroBusqueda)
        ? ListaPacientes
        : ListaPacientes.Where(p =>
            (p.Persona?.Nombre != null && p.Persona.Nombre.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase)) ||
            (p.Persona?.Apellido != null && p.Persona.Apellido.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase)) ||
            (p.NumDocumento != null && p.NumDocumento.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        await CargarObrasSociales();
        await CargarPacientes();
    }

    protected async Task CargarPacientes()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Paciente>>>("api/pacientes");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaPacientes = res.Datos;
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cargar pacientes: {ex.Message}";
        }
    }

    protected async Task CargarObrasSociales()
    {
        // Mock fallback/default Obra Social si no hay endpoint activo
        ListaObrasSociales = new List<ObraSocial>
        {
            new ObraSocial { IdObraSocial = 1, Nombre = "Particular" },
            new ObraSocial { IdObraSocial = 2, Nombre = "OSDE" },
            new ObraSocial { IdObraSocial = 3, Nombre = "Swiss Medical" },
            new ObraSocial { IdObraSocial = 4, Nombre = "Galeno" }
        };
        await Task.CompletedTask;
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
            PorcentajeIva = paciente.PorcentajeIva
        };
        MostrarModal = true;
    }

    protected async Task GuardarPaciente()
    {
        try
        {
            if (pacienteDto.IdObraSocial == 1)
            {
                pacienteDto.NumObraSocial = null;
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

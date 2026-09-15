using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.Blazor.Pages;

public partial class Agenda : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    protected Guid SelectedProfesionalId;
    protected DateOnly SelectedFecha = DateOnly.FromDateTime(DateTime.Today);

    protected List<Profesional> ListaProfesionales = new();
    protected List<Paciente> ListaPacientes = new();
    protected List<Turno> ListaTurnos = new();
    protected List<TimeOnly> TimeSlots = new();

    protected string? MensajeAlerta;
    protected bool MostrarModalNuevo = false;
    protected bool MostrarModalRecurrente = false;

    protected TurnoCreateDto nuevoTurnoDto = new();
    protected TurnoRecurrenteCreateDto recurrenteDto = new();
    protected Turno? TurnoEnEdicion;
    protected TurnoUpdateDto turnoUpdateDto = new();

    protected override async Task OnInitializedAsync()
    {
        GenerarSlotsDeTiempo();
        await CargarProfesionales();
        await CargarPacientes();
        if (SelectedProfesionalId != Guid.Empty)
        {
            await CargarTurnos();
        }
    }

    private void GenerarSlotsDeTiempo()
    {
        TimeSlots.Clear();
        var horaInicio = new TimeOnly(8, 0);
        var horaFin = new TimeOnly(20, 0);

        while (horaInicio <= horaFin)
        {
            TimeSlots.Add(horaInicio);
            horaInicio = horaInicio.AddMinutes(30);
        }
    }

    protected async Task CargarProfesionales()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Profesional>>>("api/profesionales");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaProfesionales = res.Datos;
                if (ListaProfesionales.Any() && SelectedProfesionalId == Guid.Empty)
                {
                    SelectedProfesionalId = ListaProfesionales.First().IdProfesional;
                }
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cargar profesionales: {ex.Message}";
        }
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
        catch
        {
            // Silencioso en carga secundaria
        }
    }

    protected async Task CargarTurnos()
    {
        if (SelectedProfesionalId == Guid.Empty) return;

        try
        {
            var fechaStr = SelectedFecha.ToString("yyyy-MM-dd");
            var url = $"api/turnos/profesional/{SelectedProfesionalId}?fecha={fechaStr}";
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Turno>>>(url);

            if (res != null && res.Exito && res.Datos != null)
            {
                ListaTurnos = res.Datos;
            }
            else
            {
                ListaTurnos.Clear();
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al obtener turnos: {ex.Message}";
        }
    }

    protected async Task OnProfesionalChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var guid))
        {
            SelectedProfesionalId = guid;
            await CargarTurnos();
        }
    }

    protected async Task OnFechaChanged(ChangeEventArgs e)
    {
        if (DateOnly.TryParse(e.Value?.ToString(), out var date))
        {
            SelectedFecha = date;
            await CargarTurnos();
        }
    }

    protected void AbrirModalNuevoTurno()
    {
        nuevoTurnoDto = new TurnoCreateDto
        {
            IdProfesional = SelectedProfesionalId,
            Fecha = SelectedFecha,
            Hora = new TimeOnly(9, 0)
        };
        MostrarModalNuevo = true;
    }

    protected void AgendarSlotEspecifico(TimeOnly slot)
    {
        nuevoTurnoDto = new TurnoCreateDto
        {
            IdProfesional = SelectedProfesionalId,
            Fecha = SelectedFecha,
            Hora = slot
        };
        MostrarModalNuevo = true;
    }

    protected async Task GuardarNuevoTurno()
    {
        if (nuevoTurnoDto.IdPaciente == 0)
        {
            MensajeAlerta = "Debe seleccionar un paciente.";
            return;
        }

        nuevoTurnoDto.IdProfesional = SelectedProfesionalId;
        var res = await Http.PostAsJsonAsync("api/turnos", nuevoTurnoDto);
        var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Turno>>();

        if (apiResult != null && apiResult.Exito)
        {
            MostrarModalNuevo = false;
            MensajeAlerta = "Turno agendado correctamente.";
            await CargarTurnos();
        }
        else
        {
            MensajeAlerta = apiResult?.Mensaje ?? "No se pudo agendar el turno. Verifique los solapamientos.";
        }
    }

    protected void AbrirModalTurnoRecurrente()
    {
        recurrenteDto = new TurnoRecurrenteCreateDto
        {
            IdProfesional = SelectedProfesionalId,
            DiaSemana = DayOfWeek.Monday,
            Hora = new TimeOnly(10, 0),
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            FechaFin = DateOnly.FromDateTime(DateTime.Today.AddMonths(1))
        };
        MostrarModalRecurrente = true;
    }

    protected async Task GuardarTurnosRecurrentes()
    {
        if (recurrenteDto.IdPaciente == 0)
        {
            MensajeAlerta = "Debe seleccionar un paciente.";
            return;
        }

        recurrenteDto.IdProfesional = SelectedProfesionalId;
        var res = await Http.PostAsJsonAsync("api/turnos/recurrentes", recurrenteDto);
        var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<List<Turno>>>();

        if (apiResult != null && apiResult.Exito)
        {
            MostrarModalRecurrente = false;
            MensajeAlerta = apiResult.Mensaje;
            await CargarTurnos();
        }
        else
        {
            MensajeAlerta = apiResult?.Mensaje ?? "No se pudieron agendar los turnos recurrentes.";
        }
    }

    protected void EditarEstadoTurno(Turno turno)
    {
        TurnoEnEdicion = turno;
        turnoUpdateDto = new TurnoUpdateDto
        {
            IdTurno = turno.IdTurno,
            Fecha = turno.Fecha,
            Hora = turno.Hora,
            Estado = turno.Estado,
            Observacion = turno.Observacion
        };
    }

    protected async Task GuardarEstadoTurno()
    {
        if (TurnoEnEdicion == null) return;

        var res = await Http.PutAsJsonAsync("api/turnos", turnoUpdateDto);
        var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Turno>>();

        if (apiResult != null && apiResult.Exito)
        {
            TurnoEnEdicion = null;
            MensajeAlerta = "Estado de turno actualizado.";
            await CargarTurnos();
        }
        else
        {
            MensajeAlerta = apiResult?.Mensaje ?? "Error al actualizar el turno.";
        }
    }

    protected string GetBadgeClass(EstadoTurno estado) => estado switch
    {
        EstadoTurno.Activo => "badge-activo",
        EstadoTurno.Completo => "badge-completo",
        EstadoTurno.Falta => "badge-falta",
        EstadoTurno.Cancelado => "badge-cancelado",
        _ => "badge-activo"
    };

    protected string GetPdfUrl(long idTurno) => $"http://localhost:5082/api/facturacion/comprobante/{idTurno}";
}

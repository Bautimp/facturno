using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;

namespace Facturno.Blazor.Pages;

public enum TipoVistaAgenda
{
    Dia,
    Semana,
    Mes
}

public partial class Agenda : ComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    protected Guid SelectedProfesionalId;
    protected DateOnly SelectedFecha = DateOnly.FromDateTime(DateTime.Today);
    protected TipoVistaAgenda VistaActual = TipoVistaAgenda.Dia;

    protected List<Profesional> ListaProfesionales = new();
    protected List<Paciente> ListaPacientes = new();
    protected List<Turno> ListaTurnos = new();
    protected List<TimeOnly> TimeSlots = new();

    // Vistas Semana y Mes
    protected List<DateOnly> DiasSemana = new();
    protected List<DateOnly> DiasMes = new();
    protected Dictionary<DateOnly, List<Turno>> TurnosPorFecha = new();

    protected string? MensajeAlerta;
    protected bool MostrarModalNuevo = false;
    protected bool MostrarModalRecurrente = false;

    protected TurnoCreateDto nuevoTurnoDto = new();
    protected TurnoRecurrenteCreateDto recurrenteDto = new();
    protected long? PopoverTurnoId = null;

    protected bool EsProfesionalLogueado = false;
    protected string NombreProfesionalLogueado = string.Empty;

    private bool _isLoadingTurnos = false;

    protected override async Task OnInitializedAsync()
    {
        GenerarSlotsDeTiempo();
        await VerificarRolUsuario();
        await CargarProfesionales();
        await CargarPacientes();
        if (SelectedProfesionalId != Guid.Empty)
        {
            await CargarTurnos();
        }
    }

    private async Task VerificarRolUsuario()
    {
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                EsProfesionalLogueado = user.IsInRole("Profesional");
                if (EsProfesionalLogueado)
                {
                    var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(idClaim, out var profesionalGuid))
                    {
                        SelectedProfesionalId = profesionalGuid;
                    }
                    NombreProfesionalLogueado = user.Identity.Name ?? "Mi Agenda";
                }
            }
        }
        catch
        {
            // Fallback silencioso
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

    protected async Task CambiarVista(TipoVistaAgenda nuevaVista)
    {
        if (VistaActual == nuevaVista && _isLoadingTurnos) return;
        VistaActual = nuevaVista;
        PopoverTurnoId = null;
        await CargarTurnos();
    }

    protected async Task CargarProfesionales()
    {
        try
        {
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Profesional>>>("api/profesionales");
            if (res != null && res.Exito && res.Datos != null)
            {
                ListaProfesionales = res.Datos;
                if (!EsProfesionalLogueado && ListaProfesionales.Any() && SelectedProfesionalId == Guid.Empty)
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

    protected string GetNombreProfesionalSeleccionado()
    {
        var prof = ListaProfesionales.FirstOrDefault(p => p.IdProfesional == SelectedProfesionalId);
        if (prof?.Usuario?.Persona != null)
        {
            return $"{prof.Usuario.Persona.Nombre} {prof.Usuario.Persona.Apellido} ({prof.Especialidad})";
        }
        return !string.IsNullOrWhiteSpace(NombreProfesionalLogueado) ? NombreProfesionalLogueado : "Mi Agenda";
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
            // Carga defensiva
        }
    }

    protected async Task CargarTurnos()
    {
        if (SelectedProfesionalId == Guid.Empty || _isLoadingTurnos) return;

        _isLoadingTurnos = true;
        try
        {
            PopoverTurnoId = null;

            if (VistaActual == TipoVistaAgenda.Dia)
            {
                var fechaStr = SelectedFecha.ToString("yyyy-MM-dd");
                var url = $"api/turnos/profesional/{SelectedProfesionalId}?fecha={fechaStr}";
                var res = await Http.GetFromJsonAsync<ApiResponse<List<Turno>>>(url);

                ListaTurnos = (res != null && res.Exito && res.Datos != null) ? res.Datos : new List<Turno>();
            }
            else if (VistaActual == TipoVistaAgenda.Semana)
            {
                GenerarDiasSemana();
                var diasLocales = DiasSemana.ToList();
                var tempDict = new Dictionary<DateOnly, List<Turno>>();

                foreach (var dia in diasLocales)
                {
                    var fechaStr = dia.ToString("yyyy-MM-dd");
                    var url = $"api/turnos/profesional/{SelectedProfesionalId}?fecha={fechaStr}";
                    var res = await Http.GetFromJsonAsync<ApiResponse<List<Turno>>>(url);
                    tempDict[dia] = (res != null && res.Exito && res.Datos != null) ? res.Datos : new List<Turno>();
                }

                TurnosPorFecha = tempDict;
            }
            else if (VistaActual == TipoVistaAgenda.Mes)
            {
                GenerarDiasMes();
                var diasLocales = DiasMes.ToList();
                var tempDict = new Dictionary<DateOnly, List<Turno>>();

                foreach (var dia in diasLocales)
                {
                    var fechaStr = dia.ToString("yyyy-MM-dd");
                    var url = $"api/turnos/profesional/{SelectedProfesionalId}?fecha={fechaStr}";
                    var res = await Http.GetFromJsonAsync<ApiResponse<List<Turno>>>(url);
                    if (res != null && res.Exito && res.Datos != null && res.Datos.Any())
                    {
                        tempDict[dia] = res.Datos;
                    }
                }

                TurnosPorFecha = tempDict;
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al obtener turnos: {ex.Message}";
        }
        finally
        {
            _isLoadingTurnos = false;
        }
    }

    private void GenerarDiasSemana()
    {
        DiasSemana.Clear();
        int diff = (7 + (SelectedFecha.DayOfWeek - DayOfWeek.Monday)) % 7;
        var primerDiaSemana = SelectedFecha.AddDays(-1 * diff);

        for (int i = 0; i < 7; i++)
        {
            DiasSemana.Add(primerDiaSemana.AddDays(i));
        }
    }

    protected int OffsetPrimerDiaMes = 0;

    private void GenerarDiasMes()
    {
        DiasMes.Clear();
        var primerDiaMes = new DateOnly(SelectedFecha.Year, SelectedFecha.Month, 1);
        int diasEnMes = DateTime.DaysInMonth(SelectedFecha.Year, SelectedFecha.Month);

        // Lunes = 0, Martes = 1, Miércoles = 2, Jueves = 3, Viernes = 4, Sábado = 5, Domingo = 6
        OffsetPrimerDiaMes = ((int)primerDiaMes.DayOfWeek + 6) % 7;

        for (int i = 0; i < diasEnMes; i++)
        {
            DiasMes.Add(primerDiaMes.AddDays(i));
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

    protected void TogglePopoverEstado(long idTurno)
    {
        if (PopoverTurnoId == idTurno)
        {
            PopoverTurnoId = null;
        }
        else
        {
            PopoverTurnoId = idTurno;
        }
    }

    protected void CerrarPopover()
    {
        PopoverTurnoId = null;
    }

    protected async Task CambiarEstadoDirecto(Turno turno, EstadoTurno nuevoEstado)
    {
        try
        {
            var updateDto = new TurnoUpdateDto
            {
                IdTurno = turno.IdTurno,
                Fecha = turno.Fecha,
                Hora = turno.Hora,
                Estado = nuevoEstado,
                Observacion = turno.Observacion
            };

            var res = await Http.PutAsJsonAsync("api/turnos", updateDto);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Turno>>();

            if (apiResult != null && apiResult.Exito)
            {
                turno.Estado = nuevoEstado;
                PopoverTurnoId = null;
                MensajeAlerta = $"Estado del turno #{turno.IdTurno} actualizado a {nuevoEstado}.";
                await CargarTurnos();
            }
            else
            {
                MensajeAlerta = apiResult?.Mensaje ?? "No se pudo actualizar el estado del turno.";
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al cambiar estado: {ex.Message}";
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

    protected void AbrirModalNuevoTurnoConFecha(DateOnly fecha)
    {
        nuevoTurnoDto = new TurnoCreateDto
        {
            IdProfesional = SelectedProfesionalId,
            Fecha = fecha,
            Hora = new TimeOnly(9, 0)
        };
        MostrarModalNuevo = true;
    }

    protected void AgendarSlotEspecifico(TimeOnly slot, DateOnly? fecha = null)
    {
        nuevoTurnoDto = new TurnoCreateDto
        {
            IdProfesional = SelectedProfesionalId,
            Fecha = fecha ?? SelectedFecha,
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

    protected string GetNombrePaciente(long idPaciente)
    {
        var pac = ListaPacientes.FirstOrDefault(p => p.IdPaciente == idPaciente);
        if (pac?.Persona != null)
        {
            return $"{pac.Persona.Nombre} {pac.Persona.Apellido}";
        }
        return $"Paciente #{idPaciente}";
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

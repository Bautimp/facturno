using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Facturno.Shared.Models;
using Facturno.Shared.DTOs;
using Facturno.Shared.Enums;
using Facturno.Shared.Utils;

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
    protected TipoVistaAgenda VistaActual = TipoVistaAgenda.Semana;

    protected List<Profesional> ListaProfesionales = new();
    protected List<Paciente> ListaPacientes = new();
    protected List<Turno> ListaTurnos = new();
    protected List<TimeOnly> TimeSlots = new();

    // Vista Semana
    protected List<DateOnly> DiasSemana = new();
    protected List<DateOnly> DiasMes = new();
    protected Dictionary<DateOnly, List<Turno>> TurnosPorFecha = new();
    protected EstadoTurno? FiltroEstado = null;
    protected int IndiceDiaMobile = 0;
    protected string FechaMinimaPermitidaIso => DateOnly.FromDateTime(DateTime.Today.AddDays(-30)).ToString("yyyy-MM-dd");

    protected string? MensajeAlerta;
    protected bool MostrarModalNuevo = false;
    protected bool MostrarModalRecurrente = false;

    protected TurnoCreateDto nuevoTurnoDto = new();
    protected TurnoRecurrenteCreateDto recurrenteDto = new();
    protected long? PopoverTurnoId = null;

    protected string HoraNuevoTexto
    {
        get => nuevoTurnoDto.Hora.ToString("HH:mm");
        set
        {
            if (TimeOnly.TryParse(value, out var t))
            {
                nuevoTurnoDto.Hora = t;
            }
            else if (!string.IsNullOrWhiteSpace(value) && value.Length == 4 && int.TryParse(value, out _) && TimeOnly.TryParse($"{value[..2]}:{value[2..]}", out var t2))
            {
                nuevoTurnoDto.Hora = t2;
            }
        }
    }

    protected string HoraRecurrenteTexto
    {
        get => recurrenteDto.Hora.ToString("HH:mm");
        set
        {
            if (TimeOnly.TryParse(value, out var t))
            {
                recurrenteDto.Hora = t;
            }
            else if (!string.IsNullOrWhiteSpace(value) && value.Length == 4 && int.TryParse(value, out _) && TimeOnly.TryParse($"{value[..2]}:{value[2..]}", out var t2))
            {
                recurrenteDto.Hora = t2;
            }
        }
    }

    // Búsqueda difusa de pacientes (Fuzzy Search con tolerancia a errores gramaticales)
    protected string TextoBusquedaPacienteNuevo = string.Empty;
    protected bool MostrarDropdownPacientesNuevo = false;
    protected Paciente? PacienteSeleccionadoNuevo => ListaPacientes.FirstOrDefault(p => p.IdPaciente == nuevoTurnoDto.IdPaciente);
    protected List<Paciente> PacientesFiltradosNuevo => string.IsNullOrWhiteSpace(TextoBusquedaPacienteNuevo)
        ? ListaPacientes.Where(p => p.Activo).Take(25).ToList()
        : ListaPacientes.Where(p => p.Activo && FuzzyMatcher.EsCoincidenciaPaciente(p, TextoBusquedaPacienteNuevo)).ToList();

    protected string TextoBusquedaPacienteRecurrente = string.Empty;
    protected bool MostrarDropdownPacientesRecurrente = false;
    protected Paciente? PacienteSeleccionadoRecurrente => ListaPacientes.FirstOrDefault(p => p.IdPaciente == recurrenteDto.IdPaciente);
    protected List<Paciente> PacientesFiltradosRecurrente => string.IsNullOrWhiteSpace(TextoBusquedaPacienteRecurrente)
        ? ListaPacientes.Where(p => p.Activo).Take(25).ToList()
        : ListaPacientes.Where(p => p.Activo && FuzzyMatcher.EsCoincidenciaPaciente(p, TextoBusquedaPacienteRecurrente)).ToList();

    protected void OnBusquedaPacienteNuevoInput(ChangeEventArgs e)
    {
        TextoBusquedaPacienteNuevo = e.Value?.ToString() ?? string.Empty;
        MostrarDropdownPacientesNuevo = true;
    }

    protected void SeleccionarPacienteNuevo(Paciente pac)
    {
        nuevoTurnoDto.IdPaciente = pac.IdPaciente;
        TextoBusquedaPacienteNuevo = $"{pac.Persona?.Nombre} {pac.Persona?.Apellido}".Trim();
        MostrarDropdownPacientesNuevo = false;
        StateHasChanged();
    }

    protected void LimpiarSeleccionPacienteNuevo()
    {
        nuevoTurnoDto.IdPaciente = 0;
        TextoBusquedaPacienteNuevo = string.Empty;
        MostrarDropdownPacientesNuevo = true;
        StateHasChanged();
    }

    protected void OnBusquedaPacienteRecurrenteInput(ChangeEventArgs e)
    {
        TextoBusquedaPacienteRecurrente = e.Value?.ToString() ?? string.Empty;
        MostrarDropdownPacientesRecurrente = true;
    }

    protected void SeleccionarPacienteRecurrente(Paciente pac)
    {
        recurrenteDto.IdPaciente = pac.IdPaciente;
        TextoBusquedaPacienteRecurrente = $"{pac.Persona?.Nombre} {pac.Persona?.Apellido}".Trim();
        MostrarDropdownPacientesRecurrente = false;
        StateHasChanged();
    }

    protected void LimpiarSeleccionPacienteRecurrente()
    {
        recurrenteDto.IdPaciente = 0;
        TextoBusquedaPacienteRecurrente = string.Empty;
        MostrarDropdownPacientesRecurrente = true;
        StateHasChanged();
    }

    protected bool EsProfesionalLogueado = false;
    protected string NombreProfesionalLogueado = string.Empty;

    protected bool IsLoading = false;

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
                    var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                               ?? user.FindFirst("id_usuario")?.Value
                               ?? user.FindFirst("sub")?.Value;
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
        VistaActual = nuevaVista;
        PopoverTurnoId = null;
        await CargarTurnos();
        StateHasChanged();
    }

    protected async Task SemanaAnterior()
    {
        SelectedFecha = SelectedFecha.AddDays(-7);
        await CargarTurnos();
    }

    protected async Task SemanaSiguiente()
    {
        SelectedFecha = SelectedFecha.AddDays(7);
        await CargarTurnos();
    }

    protected async Task IrSemanaActual()
    {
        SelectedFecha = DateOnly.FromDateTime(DateTime.Today);
        await CargarTurnos();
    }

    protected string GetTextoRangoSemana()
    {
        if (DiasSemana.Count < 7) return string.Empty;
        var inicio = DiasSemana[0];
        var fin = DiasSemana[6];
        var cult = new System.Globalization.CultureInfo("es-ES");
        if (inicio.Month == fin.Month && inicio.Year == fin.Year)
        {
            return $"Semana del {inicio.Day} al {fin.Day} de {fin.ToString("MMMM", cult)} {fin.Year}";
        }
        if (inicio.Year == fin.Year)
        {
            return $"Semana del {inicio.Day} de {inicio.ToString("MMMM", cult)} al {fin.Day} de {fin.ToString("MMMM", cult)} {fin.Year}";
        }
        return $"Semana del {inicio.Day}/{inicio.Month}/{inicio.Year} al {fin.Day}/{fin.Month}/{fin.Year}";
    }

    protected void OnFiltroEstadoChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(val) || !Enum.TryParse<EstadoTurno>(val, out var parsed))
        {
            FiltroEstado = null;
        }
        else
        {
            FiltroEstado = parsed;
        }
    }

    protected List<Turno> ObtenerTurnosFiltradosPorDia(DateOnly dia)
    {
        if (!TurnosPorFecha.TryGetValue(dia, out var lista))
        {
            return new List<Turno>();
        }

        if (FiltroEstado == null)
        {
            return lista;
        }

        return lista.Where(t => t.Estado == FiltroEstado.Value).ToList();
    }

    protected void SeleccionarDiaMobile(int idx)
    {
        if (idx >= 0 && idx < DiasSemana.Count)
        {
            IndiceDiaMobile = idx;
        }
    }

    protected int GetTotalTurnosSemana()
    {
        if (FiltroEstado == null)
        {
            return TurnosPorFecha.Values.Sum(list => list.Count);
        }
        return TurnosPorFecha.Values.Sum(list => list.Count(t => t.Estado == FiltroEstado.Value));
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
            if (!EsProfesionalLogueado)
            {
                MensajeAlerta = $"Error al cargar profesionales: {ex.Message}";
            }
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
        if (SelectedProfesionalId == Guid.Empty) return;

        IsLoading = true;
        try
        {
            PopoverTurnoId = null;
            GenerarDiasSemana();
            var diasLocales = DiasSemana.ToList();
            if (!diasLocales.Any()) return;

            var inicio = diasLocales.First().ToString("yyyy-MM-dd");
            var fin = diasLocales.Last().ToString("yyyy-MM-dd");

            // Una única petición HTTP optimizada para toda la semana
            var url = $"api/turnos/profesional/{SelectedProfesionalId}?fechaInicio={inicio}&fechaFin={fin}";
            var res = await Http.GetFromJsonAsync<ApiResponse<List<Turno>>>(url);

            var tempDict = new Dictionary<DateOnly, List<Turno>>();
            var turnosSemana = (res != null && res.Exito && res.Datos != null)
                ? res.Datos
                : new List<Turno>();

            var turnosAgrupados = turnosSemana
                .GroupBy(t => t.Fecha)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Hora).ToList());

            foreach (var dia in diasLocales)
            {
                tempDict[dia] = turnosAgrupados.TryGetValue(dia, out var lista) ? lista : new List<Turno>();
            }

            TurnosPorFecha = tempDict;
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al obtener turnos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
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

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var idxHoy = DiasSemana.IndexOf(hoy);
        if (idxHoy >= 0)
        {
            IndiceDiaMobile = idxHoy;
        }
        else if (IndiceDiaMobile >= DiasSemana.Count)
        {
            IndiceDiaMobile = 0;
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

    protected bool mostrarModalEstado = false;
    protected Turno? turnoSeleccionado;
    protected EstadoTurno estadoSeleccionado;

    protected bool editandoFechaHora = false;
    protected bool editandoModalidad = false;
    protected bool editandoObservacion = false;
    protected DateOnly editFecha;
    protected TimeOnly editHora;
    protected ModalidadTurno editModalidad = ModalidadTurno.Presencial;
    protected string? editEnlaceReunion;
    protected string? editObservacion;

    protected string HoraEditTexto
    {
        get => editHora.ToString("HH:mm");
        set
        {
            if (TimeOnly.TryParse(value, out var t))
            {
                editHora = t;
            }
            else if (!string.IsNullOrWhiteSpace(value) && value.Length == 4 && int.TryParse(value, out _) && TimeOnly.TryParse($"{value[..2]}:{value[2..]}", out var t2))
            {
                editHora = t2;
            }
        }
    }

    protected void ToggleEditarFechaHora() => editandoFechaHora = !editandoFechaHora;
    protected void ToggleEditarModalidad() => editandoModalidad = !editandoModalidad;
    protected void ToggleEditarObservacion() => editandoObservacion = !editandoObservacion;

    protected void AbrirModalEstado(Turno turno)
    {
        turnoSeleccionado = turno;
        estadoSeleccionado = turno.Estado;
        editFecha = turno.Fecha;
        editHora = turno.Hora;
        editModalidad = turno.Modalidad;
        editEnlaceReunion = turno.EnlaceReunion;
        editObservacion = turno.Observacion;
        editandoFechaHora = false;
        editandoModalidad = false;
        editandoObservacion = false;
        mostrarModalEstado = true;
        StateHasChanged();
    }

    protected void CerrarModalEstado()
    {
        mostrarModalEstado = false;
        turnoSeleccionado = null;
        editandoFechaHora = false;
        editandoModalidad = false;
        editandoObservacion = false;
        StateHasChanged();
    }

    protected async Task GuardarEstado()
    {
        if (turnoSeleccionado == null) return;

        try
        {
            var horaMinima = new TimeOnly(6, 0);
            var horaMaxima = new TimeOnly(23, 0);
            if (editHora < horaMinima || editHora > horaMaxima)
            {
                MensajeAlerta = "El horario del turno debe estar comprendido entre las 06:00 y las 23:00 hs.";
                return;
            }

            var fechaMinima = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            if (editFecha < fechaMinima)
            {
                MensajeAlerta = "No se pueden agendar turnos con más de 30 días de antigüedad respecto al día de hoy.";
                return;
            }

            var updateDto = new TurnoUpdateDto
            {
                IdTurno = turnoSeleccionado.IdTurno,
                Fecha = editFecha,
                Hora = editHora,
                Estado = estadoSeleccionado,
                Observacion = editObservacion,
                Modalidad = editModalidad,
                EnlaceReunion = editEnlaceReunion
            };

            var res = await Http.PutAsJsonAsync("api/turnos", updateDto);
            var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<Turno>(res, "No se pudo actualizar el turno.");

            if (exito)
            {
                turnoSeleccionado.Estado = estadoSeleccionado;
                turnoSeleccionado.Fecha = editFecha;
                turnoSeleccionado.Hora = editHora;
                turnoSeleccionado.Modalidad = editModalidad;
                turnoSeleccionado.EnlaceReunion = editEnlaceReunion;
                turnoSeleccionado.Observacion = editObservacion;

                var nombrePaciente = GetNombrePaciente(turnoSeleccionado.IdPaciente);
                MensajeAlerta = $"Turno de {nombrePaciente} actualizado correctamente.";
                CerrarModalEstado();
                await CargarTurnos();
            }
            else
            {
                MensajeAlerta = mensaje;
            }
        }
        catch (Exception ex)
        {
            MensajeAlerta = $"Error al guardar cambios del turno: {ex.Message}";
        }
        finally
        {
            StateHasChanged();
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
        StateHasChanged();
    }

    protected void CerrarPopover()
    {
        PopoverTurnoId = null;
        StateHasChanged();
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
                Observacion = turno.Observacion,
                Modalidad = turno.Modalidad,
                EnlaceReunion = turno.EnlaceReunion
            };

            var res = await Http.PutAsJsonAsync("api/turnos", updateDto);
            var apiResult = await res.Content.ReadFromJsonAsync<ApiResponse<Turno>>();

            if (apiResult != null && apiResult.Exito)
            {
                turno.Estado = nuevoEstado;
                PopoverTurnoId = null;
                var nombrePaciente = GetNombrePaciente(turno.IdPaciente);
                MensajeAlerta = $"Estado del turno de {nombrePaciente} actualizado a {nuevoEstado}.";
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
        finally
        {
            StateHasChanged();
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
        TextoBusquedaPacienteNuevo = string.Empty;
        MostrarDropdownPacientesNuevo = false;
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
        TextoBusquedaPacienteNuevo = string.Empty;
        MostrarDropdownPacientesNuevo = false;
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
        TextoBusquedaPacienteNuevo = string.Empty;
        MostrarDropdownPacientesNuevo = false;
        MostrarModalNuevo = true;
    }

    protected async Task GuardarNuevoTurno()
    {
        if (nuevoTurnoDto.IdPaciente == 0)
        {
            MensajeAlerta = "Debe seleccionar un paciente.";
            return;
        }

        var horaMinima = new TimeOnly(6, 0);
        var horaMaxima = new TimeOnly(23, 0);
        if (nuevoTurnoDto.Hora < horaMinima || nuevoTurnoDto.Hora > horaMaxima)
        {
            MensajeAlerta = "El horario del turno debe estar comprendido entre las 06:00 y las 23:00 hs.";
            return;
        }

        var fechaMinima = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        if (nuevoTurnoDto.Fecha < fechaMinima)
        {
            MensajeAlerta = "No se pueden agendar turnos con más de 30 días de antigüedad respecto al día de hoy.";
            return;
        }

        nuevoTurnoDto.IdProfesional = SelectedProfesionalId;
        var res = await Http.PostAsJsonAsync("api/turnos", nuevoTurnoDto);
        var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<Turno>(res, "No se pudo agendar el turno. Verifique los solapamientos.");

        if (exito)
        {
            MostrarModalNuevo = false;
            MensajeAlerta = "Turno agendado correctamente.";
            await CargarTurnos();
        }
        else
        {
            MensajeAlerta = mensaje;
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
        TextoBusquedaPacienteRecurrente = string.Empty;
        MostrarDropdownPacientesRecurrente = false;
        MostrarModalRecurrente = true;
    }

    protected async Task GuardarTurnosRecurrentes()
    {
        if (recurrenteDto.IdPaciente == 0)
        {
            MensajeAlerta = "Debe seleccionar un paciente.";
            return;
        }

        var horaMinima = new TimeOnly(6, 0);
        var horaMaxima = new TimeOnly(23, 0);
        if (recurrenteDto.Hora < horaMinima || recurrenteDto.Hora > horaMaxima)
        {
            MensajeAlerta = "El horario del turno debe estar comprendido entre las 06:00 y las 23:00 hs.";
            return;
        }

        var fechaMinima = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        if (recurrenteDto.FechaInicio < fechaMinima)
        {
            MensajeAlerta = "La fecha de inicio de la serie recurrente no puede superar los 30 días de antigüedad.";
            return;
        }

        recurrenteDto.IdProfesional = SelectedProfesionalId;
        var res = await Http.PostAsJsonAsync("api/turnos/recurrentes", recurrenteDto);
        var (exito, mensaje, datos) = await ApiResponseParser.ProcesarRespuestaAsync<List<Turno>>(res, "No se pudieron agendar los turnos recurrentes.");

        if (exito)
        {
            MostrarModalRecurrente = false;
            MensajeAlerta = !string.IsNullOrWhiteSpace(mensaje) ? mensaje : "Serie de turnos recurrentes agendada correctamente.";
            await CargarTurnos();
        }
        else
        {
            MensajeAlerta = mensaje;
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

    protected string GetEstadoTextoConIcono(EstadoTurno estado) => estado switch
    {
        EstadoTurno.Activo => "Activo",
        EstadoTurno.Completo => "Completo",
        EstadoTurno.Falta => "Falta",
        EstadoTurno.Cancelado => "Cancelado",
        _ => estado.ToString()
    };

    protected string GetBorderAccentClass(EstadoTurno estado) => estado switch
    {
        EstadoTurno.Activo => "border-estado-activo",
        EstadoTurno.Completo => "border-estado-completo",
        EstadoTurno.Falta => "border-estado-falta",
        EstadoTurno.Cancelado => "border-estado-cancelado",
        _ => "border-estado-activo"
    };

    protected string GetPdfUrl(long idTurno) => $"{Http.BaseAddress?.ToString().TrimEnd('/')}/api/facturacion/comprobante/{idTurno}";
}

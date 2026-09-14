# Pasos Siguientes y Estado de Avance del Sistema Facturno

Este documento resume el avance actual del desarrollo del sistema Facturno y la hoja de ruta para completar las próximas fases.

---

## 🟢 Estado de Avance Actual

### ✅ Fase 1: Modelado del Dominio (Capa Core / `Facturno.Shared`) - COMPLETADO
- **Entidades de Dominio (`DomainModels.cs`)**: Creadas e implementadas en C# (`Persona`, `Usuario`, `Profesional`, `Paciente`, `Turno`, `ObraSocial`, `AgendaCompartida`).
- **Identificadores UUID / int8**: Sincronizados con Supabase Auth (`id_usuario` y `id_profesional` como `Guid`/`UUID`).
- **Enums del Dominio (`DomainEnums.cs`)**: Sincronizados exactamente con los tipos enumerados de Supabase (`RolUsuario`, `EstadoTurno` (`Activo`, `Completo`, `Falta`, `Cancelado`), `TipoDocumento`, `TipoEspecialidad`, `CondicionIVA`, `TipoComprobante`).
- **Data Transfer Objects (`DomainDtos.cs`)**: Creados los DTOs para solicitudes de API (`TurnoCreateDto`, `TurnoUpdateDto`, `TurnoRecurrenteCreateDto`, `PacienteCreateDto`, `ProfesionalCreateDto`, `UsuarioLoginDto`).
- **Wrapper de Respuesta (`ApiResponse.cs`)**: Formato de respuesta JSON estandarizado `{ exito: bool, datos: T, mensaje: string, errores: List<string> }`.
- **Contratos de Interfaz (`Interfaces/`)**: Definidos `ITurnoRepository`, `IPacienteRepository`, `IProfesionalRepository`, `IUsuarioRepository`, `IObraSocialRepository` e `ITurnoService`.

### ✅ Fase 2: Persistencia y Esquema de Base de Datos (Supabase PostgreSQL) - COMPLETADO
- **Tablas e Índices**: Esquema desplegado y verificado en Supabase.
- **Índice Parcial de Solapamiento**: `idx_turnos_profesional_fecha_hora_unicos` (`WHERE estado != 'Cancelado'`).
- **Clave Primaria Compuesta**: `agendas_compartidas(id_administrativo, id_profesional)` configurada.
- **Trigger de Baja Lógica**: `trg_baja_profesional_cancelar_turnos` para cancelación automática de turnos futuros activos al desactivar a un profesional.

### ✅ Fase 3: Servicios de Lógica de Negocio y Backend REST API (`Facturno.API` & `Facturno.Infrastructure`) - COMPLETADO
- **Repositorios Supabase**: Implementados `TurnoRepository`, `PacienteRepository`, `ProfesionalRepository`, `UsuarioRepository` y `ObraSocialRepository`.
- **`TurnoService.cs`**: Implementado con validación de solapamientos en rango de 30 minutos mínimos y agendamiento periódico recurrente (`TurnoRecurrenteCreateDto`).
- **Servicio PDF al Vuelo**: `FacturaPdfService.cs` implementado con QuestPDF para la generación dinámica de comprobantes de atención en formato PDF.
- **Controladores REST**: `TurnosController.cs`, `PacientesController.cs`, `ProfesionalesController.cs`, `AuthController.cs` y `FacturacionController.cs` con atributos `[Authorize]` y envoltorio `ApiResponse<T>`.
- **Configuración IoC y Auth**: `Program.cs` configurado con inyección de dependencias, CORS, autenticación y Swagger/OpenAPI.

---

## 🟡 Próximos Pasos Inmediatos (Milestone 4 - Frontend Blazor WebAssembly)

### 1. Componentes y Vistas Blazor WebAssembly (Issue #7 - #11, #14, #15)
- Configurar `DelegatingHandler` en Blazor para inyección transparente del encabezado `Authorization: Bearer <JWT>`.
- Construir vistas con el patrón Code-Behind (`.razor` + `.razor.cs`):
  - `Login.razor`
  - `Agenda.razor`
  - `Pacientes.razor`
  - `GestionPersonal.razor`
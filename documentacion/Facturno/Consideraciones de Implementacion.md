# Consideraciones de Implementación y Arquitectura - Sistema Facturno

Este documento recopila las recomendaciones técnicas, riesgos identificados y buenas prácticas a tener en cuenta durante el desarrollo del backend (.NET) y frontend (Blazor) del sistema Facturno.

---

## 1. Transaccionalidad en el Alta de Entidades (`personas` → `usuarios` → `profesionales` / `pacientes`)

### Diagnóstico
El modelo de datos está normalizado en entidades heredadas/relacionadas a través de claves foráneas (`FK`):
- `personas` contiene los datos filiatorios básicos.
- `usuarios` extiende a `personas` para otorgar credenciales y rol.
- `profesionales` y `pacientes` extienden a `usuarios` / `personas`.

### Consideraciones de Desarrollo
- Al registrar un nuevo usuario (ej. `Profesional`), la inserción debe realizarse en **cascada atómica**. Si la inserción en `profesionales` falla, debe ejecutarse un `ROLLBACK` de las inserciones en `usuarios` y `personas`.
- **Estrategia Recomendada**:
  - Implementar transacciones en la capa de servicios de C# usando `DbContext.Database.BeginTransactionAsync()` o funciones almacenadas (`RPC`) en PostgreSQL/Supabase.
  - Sincronizar el `id_usuario` con el ID generado por `auth.users` de Supabase Auth mediante triggers o servicios de autenticación centralizados.

---

## 2. Prevención de Solapamiento de Turnos y Concurrencia

### Diagnóstico
Dos operadores o usuarios podrían intentar agendar un turno para el mismo profesional a la misma fecha y hora de forma simultánea (condición de carrera).

### Consideraciones de Desarrollo
- Se ha incorporado en la base de datos el **índice de exclusión parcial**:
  ```sql
  CREATE UNIQUE INDEX idx_turnos_profesional_fecha_hora_unicos 
  ON turnos (id_profesional, fecha, hora) 
  WHERE estado != 'Cancelado';
  ```
- **Estrategia Recomendada**:
  - En la capa de aplicación (`ITurnoService.cs`), realizar una validación previa de disponibilidad antes de intentar la inserción.
  - Capturar adecuadamente la excepción de violación de índice único (`DbUpdateException` / `PostgresException`) en el backend para retornar un mensaje amigable al usuario (*"El profesional ya cuenta con un turno asignado en dicho horario"*).

---

## 3. Control de Acceso sobre Agendas Compartidas (`agendas_compartidas`)

### Diagnóstico
Los usuarios con rol `Administrativo` solo deben visualizar y gestionar las agendas de aquellos profesionales a los cuales se les ha concedido permiso explícito a través de la tabla `agendas_compartidas`.

### Consideraciones de Desarrollo
- **Estrategia Recomendada**:
  - Encapsular las consultas de turnos en un método de repositorio/servicio que verifique el rol del usuario autenticado:
    - Si es `Profesional`: solo ve sus turnos (`id_profesional == idUsuario`).
    - Si es `Administrativo`: solo ve los turnos de profesionales vinculados (`id_profesional IN (SELECT id_profesional FROM agendas_compartidas WHERE id_administrativo = idUsuario)`).
    - Si es `Operador`: tiene acceso global de gestión.
  - Opcionalmente, configurar políticas de **Row Level Security (RLS)** en Supabase.

---

## 4. Integración con ARCA / AFIP (Webservices WSFEv1) y Resiliencia

### Diagnóstico
La emisión de facturas electrónicas requiere comunicación con los servidores SOAP de la AFIP, los cuales pueden experimentar latencias o caídas temporales de servicio.

### Consideraciones de Desarrollo
- **Estrategia Recomendada**:
  - Desacoplar la atención del turno de la emisión fiscal: la atención se registra inmediatamente en la base de datos.
  - La facturación se maneja **únicamente mediante la generación de archivos PDF al vuelo** (o integración directa sin almacenamiento de comprobantes en la base de datos). No se requiere persistencia en tabla de comprobantes en Supabase.
  - En caso de indisponibilidad del servicio de AFIP, utilizar la generación de **comprobantes internos / Factura PDF** como contingencia (*fallback*).
  - Mantener la gestión de certificados digitales (`.p12` / `.crt`) y CUIT profesional en variables de entorno o entidad `profesionales`.

---

## 5. Mapeo de Tipos de Datos y Máscaras de Entrada

### Diagnóstico
- Los campos `num_documento` y `num_obra_social` están definidos como **`text`** en Supabase para soportar pasaportes alfanuméricos y CUIT/CUIL con guiones.
- El teléfono se mantiene como `int8` (bigint).
- Fechas y horas están segregadas en tipos `date` y `time`.

### Consideraciones de Desarrollo
- **Estrategia Recomendada**:
  - En C# .NET 6+, mapear `fecha` a `DateOnly` y `hora` a `TimeOnly` para evitar desfasajes por zonas horarias (UTC vs Hora local).
  - Mapear `num_documento` y `num_obra_social` como `string` en las entidades C# .NET.
  - En las vistas de Blazor, aplicar máscaras de validación dinámicas según el tipo de documento seleccionado (`DNI`, `Pasaporte` alfanumérico, `CUIL`).

---

## 6. Seguridad de Endpoints (Backend API vs Frontend Blazor)

### Diagnóstico
La interfaz visual de Blazor puede ocultar botones o navegación según el rol, pero la seguridad real debe residir en el servidor.

### Consideraciones de Desarrollo
- **Estrategia Recomendada**:
  - Decorar todos los controladores y endpoints de C# Web API con atributos de autorización específicos por rol: `[Authorize(Roles = "Profesional")]`, `[Authorize(Roles = "Operador")]`, etc.
  - No confiar únicamente en la validación del lado del cliente.

---

## 7. Reglas de Negocio Específicas y Resolución de Puntos Débiles

### Diagnóstico y Estrategias Consensuadas
- **Autenticación (Supabase Auth UUID)**: `usuarios` y `profesionales` utilizan identificadores de tipo `UUID` vinculados a `auth.users.id`. La tabla `usuarios` referencia a `personas.id_persona` (`int8`) para almacenar datos filiatorios.
- **Control de Solapamiento (30 Minutos)**: Los turnos tienen una duración estándar/mínima de 30 minutos. La capa de negocio (`TurnoService.cs`) validará que no exista ningún turno activo para el mismo profesional en el rango `[hora, hora + 30 min]`.
- **Precios de Consulta**: El monto no se duplica en la tabla `turnos`; se consulta directamente desde `profesionales.precio_consulta`. En la UI de Blazor se notifica al profesional que modificar su tarifa afectará la facturación de sus consultas pendientes.
- **Baja Lógica de Profesional**: Al cambiar `usuarios.activo = false` para un profesional, el servicio cancelará automáticamente todos sus turnos futuros en estado `Activo`.
- **Cobertura de Obra Social**: Las consultas heredan directamente la obra social asignada al perfil del paciente (`pacientes.id_obra_social`).

---

## 8. Patrones Arquitectónicos y de Diseño del Sistema

### Definiciones Estructurales y de Código
1. **Organización en 4 Proyectos .NET**:
   - `Facturno.Shared` (`src/Core/Facturno.Shared`): Entidades de dominio, DTOs, Enums, contratos e `ApiResponse<T>`.
   - `Facturno.Infrastructure` (`src/Infraestructura/Facturno.Infrastructure`): Repositorios C# con SDK `supabase-csharp`, cliente SOAP ARCA y generador PDF.
   - `Facturno.API` (`src/API/Facturno.API`): Controllers RESTful, autenticación y middleware.
   - `Facturno.Blazor` (`src/UI/Facturno.Blazor`): Cliente Blazor WebAssembly.

2. **Patrón de Presentación en Blazor (Code-Behind)**:
   - Separación estricta de archivos `.razor` para UI/HTML y `.razor.cs` para clases parciales C# con la lógica de componente e invocaciones HTTP.

3. **Patrón de Acceso a Datos (Repositorios Específicos)**:
   - Uso de interfaces `ITurnoRepository`, `IPacienteRepository`, `IProfesionalRepository`, `IUsuarioRepository` e `IObraSocialRepository` para desacoplar el SDK de Supabase.

4. **Formato de Respuesta Web API (`ApiResponse<T>`)**:
   - Estructuración JSON uniforme en todos los endpoints REST: `{ exito: bool, datos: T, mensaje: string, errores: List<string> }`.

5. **Inyección de Tokens JWT (`DelegatingHandler`)**:
   - Registro de `AuthorizationHandler : DelegatingHandler` en Blazor para adjuntar de forma transparente el encabezado `Authorization: Bearer <token>` a las llamadas HTTP salientes hacia la API.

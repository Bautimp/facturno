# Planificación de Incrementos e Issues (GitHub) - Sistema Facturno

Este documento establece la distribución del desarrollo del sistema Facturno en 3 Incrementos (Sprints), organizados como Issues de GitHub listos para su creación y seguimiento.

---

## Definiciones Arquitectónicas del Proyecto
- **Estilo Arquitectónico**: Arquitectura Limpia Multicapa (.NET Web API + Blazor WebAssembly + Domain + Application + Infrastructure).
- **Frontend**: Blazor WebAssembly (.NET 10.0 LTS).
- **Backend / Web API**: ASP.NET Core Web API C#.
- **Persistencia**: Supabase PostgreSQL mediante el cliente oficial `supabase-csharp` API REST.
- **Autenticación**: Supabase Auth (Email/Contraseña + Google OAuth).

---

## INCREMENTO 1: Arquitectura Base, Autenticación y Módulo Core de Turnos
**Fecha de Entrega**: 15/09/2026  
**Casos de Uso**: `CU1`, `CU2`, `CU3`, `CU4`, `CU5`, `CU6`, `CU7`, `CU8`, `CU9`, `CU10`, `CU11`, `CU12`, `CU18`, `CU19`, `CU20`, `CU21`, `CU22`, `CU23`, `CU24`.

### Issue #1: Configuración de la Solución Multicapa .NET y Blazor WebAssembly
- **Tipo**: Infrastructure / Chore
- **Descripción**: Crear la estructura de la solución C# (`Facturno.sln`) con los proyectos `Facturno.Domain`, `Facturno.Application`, `Facturno.Infrastructure`, `Facturno.Api` y `Facturno.Client` (Blazor WebAssembly).
- **Criterios de Aceptación**:
  - [ ] Solución creada con compilación limpia.
  - [ ] Inyección de dependencias configurada entre capas.
  - [ ] Proyecto `Facturno.Client` consumiendo un endpoint de prueba en `Facturno.Api`.

### Issue #2: Configuración de Supabase Client SDK e Índices de Integridad
- **Tipo**: Database / Feature
- **Descripción**: Configurar la conexión a Supabase utilizando la librería oficial `supabase-csharp` e implementar los scripts de migración de tablas e índices.
- **Criterios de Aceptación**:
  - [ ] Tablas `personas`, `usuarios`, `profesionales`, `pacientes`, `turnos`, `obras_sociales`, `agendas_compartidas` creadas.
  - [ ] Índice único parcial `idx_turnos_profesional_fecha_hora_unicos` (`WHERE estado != 'Cancelado'`) aplicado en PostgreSQL.
  - [ ] Cliente `Supabase.Client` registrado en el contenedor IoC del backend.

### Issue #3: Módulo de Autenticación con Supabase Auth (Email y Google OAuth)
- **Tipo**: Feature / Auth
- **Casos de Uso**: `CU1: Registrarse`, `CU2: Iniciar Sesión`, `CU3: Recuperar contraseña`, `CU4: Cerrar Sesión`
- **Descripción**: Implementar el flujo de autenticación seguro en Blazor WebAssembly respaldado por Supabase Auth.
- **Criterios de Aceptación**:
  - [ ] Login con Email y Contraseña.
  - [ ] Login con proveedor externo Google OAuth.
  - [ ] Creación e inclusión del perfil de usuario en `public.usuarios` con asignación de rol (`Administrativo`, `Profesional`, `Operador`).
  - [ ] Implementación de `AuthenticationStateProvider` en Blazor para la protección de vistas.

### Issue #4: ABM de Personal (Profesionales y Administrativos)
- **Tipo**: Feature
- **Casos de Uso**: `CU18`, `CU19`, `CU20`, `CU21`, `CU22`, `CU23`
- **Descripción**: Permitir al Operador crear, modificar y dar de baja lógica a profesionales y administrativos.
- **Criterios de Aceptación**:
  - [ ] Transacción atómica en `personas` + `usuarios` + `profesionales` / `administrativos`.
  - [ ] Alta de profesional registrando matrícula y especialidad.
  - [ ] Baja lógica mediante el campo `activo = false`.

### Issue #5: Gestión de Permisos de Agendas Compartidas
- **Tipo**: Feature
- **Casos de Uso**: `CU24: Vincular agenda de profesional a administrativo`
- **Descripción**: Permitir al Operador autorizar a un usuario administrativo a gestionar la agenda de uno o varios profesionales.
- **Criterios de Aceptación**:
  - [ ] Inserción y borrado en la tabla `agendas_compartidas`.
  - [ ] Pantalla de asignación de permisos en Blazor.

### Issue #6: Gestión de Pacientes y Obras Sociales
- **Tipo**: Feature
- **Casos de Uso**: `CU5`, `CU6: Agregar obra social`, `CU7`, `Modificar obra social`, `CU11`, `Ver historial paciente`
- **Descripción**: Permitir la registración, edición y consulta de fichas de pacientes y sus respectivas obras sociales.
- **Criterios de Aceptación**:
  - [ ] Formulario de alta/edición de pacientes con validaciones para campos `int8` (DNI, Teléfono).
  - [ ] ABM de Obra Social y asignación de número de afiliado.
  - [ ] Consulta de datos del paciente e historial.

### Issue #7: Calendario, Agendamiento y Control de Solapamientos de Turnos
- **Tipo**: Feature / Core
- **Casos de Uso**: `CU8: Agregar un turno`, `CU9: Modificar un turno`, `CU10: Eliminar turno`, `CU12: Ver agenda`
- **Descripción**: Implementar el módulo de agenda con vista de calendario interactivo para consultar, reservar, reagendar y cancelar turnos.
- **Criterios de Aceptación**:
  - [ ] Vista de agenda diaria/semanal en Blazor WebAssembly (`CU12`).
  - [ ] Agendamiento de turnos validando previamente solapamientos en `ITurnoService`.
  - [ ] Manejo del error de violación del índice único parcial en PostgreSQL.
  - [ ] Actualización de estado del turno (`Pendiente`, `Finalizado`, `Ausente`, `Cancelado`).

---

## INCREMENTO 2: Módulo de Facturación Electrónica (ARCA / AFIP)
**Fecha de Entrega**: 10/10/2026  
**Casos de Uso**: `CU13`, `CU14`, `CU15`, `CU16`, `CU17`.

### Issue #8: Parametrización Fiscal del Profesional
- **Tipo**: Feature
- **Casos de Uso**: `CU14: Modificar precio de consulta`
- **Descripción**: Permitir al Profesional configurar el valor base de consulta y sus datos impositivos (Condición IVA, Tipo de Comprobante A/B).
- **Criterios de Aceptación**:
  - [ ] Formulario de configuración de honorarios y datos de facturación en el perfil del profesional.

### Issue #9: Integración con Web Services ARCA / AFIP (WSFEv1)
- **Tipo**: Feature / Integration
- **Casos de Uso**: `CU13: Facturar una sesión`, `CU17: Solicitar factura`
- **Descripción**: Conectar el backend de Facturno con los Web Services de ARCA (AFIP Homologación) para emitir facturas electrónicas A y B.
- **Criterios de Aceptación**:
  - [ ] Servicio de autenticación WSAA y emisión WSFEv1 en C#.
  - [ ] Emisión de factura al finalizar una consulta con obtención del CAE y fecha de vencimiento.

### Issue #10: Emisión de Comprobante PDF (Mecanismo de Contingencia)
- **Tipo**: Feature / Contingency
- **Casos de Uso**: `CU13: Facturar sesión (Modo Contingencia)`
- **Descripción**: Generar una factura/recibo en PDF si el servicio de AFIP no responde para no demorar la atención del paciente.
- **Criterios de Aceptación**:
  - [ ] Plantilla PDF estructurada de comprobante de consulta.
  - [ ] Almacenamiento del PDF en Supabase Storage.

---

## INCREMENTO 3: Turnos Recurrentes, Métricas y Cierre de Proyecto
**Fecha de Entrega**: 06/11/2026  
**Casos de Uso**: Cierre integral de todos los Casos de Uso (`CU1` a `CU24`).

### Issue #11: Programación de Turnos Recurrentes
- **Tipo**: Feature
- **Casos de Uso**: `CU8: Agregar turnos (Recurrencia)`
- **Descripción**: Permitir la generación de múltiples turnos periódicos para tratamientos prolongados en una sola transacción.
- **Criterios de Aceptación**:
  - [ ] Asignación de turnos repetitivos (semanal / quincenal) con informe de conflictos.

### Issue #12: Tablero de Historial y Métricas de Ausentismo
- **Tipo**: Feature / Analytics
- **Casos de Uso**: `CU11: Ver historial paciente`
- **Descripción**: Brindar métricas sobre la tasa de ausentismo e historial de consultas del paciente para la toma de decisiones.
- **Criterios de Aceptación**:
  - [ ] Panel con indicadores de asistencias, inasistencias y cancelación de turnos.

### Issue #13: QA, Pruebas de Integración y Documentación Técnica Final
- **Tipo**: QA / Documentation
- **Casos de Uso**: Todos
- **Descripción**: Ejecutar pruebas de carga/integración, ajustar el diseño responsive en Blazor y finalizar la documentación.
- **Criterios de Aceptación**:
  - [ ] Pruebas end-to-end en entorno de staging completadas.
  - [ ] Manual de usuario y documentación técnica de la solución.
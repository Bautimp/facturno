Para el sistema Facturno se establece una base de datos en el sitio Supabase, siendo ésta la estructura de la misma:

# Tablas

## Table `personas`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_persona` | `int8` | Primary Identity | Clave primaria |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `nombre` | `text` | Nullable | Nombre de la persona |
| `apellido` | `text` | Nullable | Apellido de la persona |
| `correo` | `text` | Nullable Unique | Correo electrónico |
| `telefono` | `int8` | Nullable | Teléfono |

## Table `usuarios`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_usuario` | `uuid` | Primary Key | FK `auth.users.id` Not Null | ID de usuario de Supabase Auth |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `id_persona` | `int8` | FK `personas.id_persona` Not Null | Vinculación con datos filiatorios |
| `rol` | `RolUsuario` | Nullable | Rol (`Administrativo`, `Profesional`, `Operador`) |
| `activo` | `bool` | Default `true` Not Null | Estado activo/inactivo del usuario |

## Table `profesionales`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_profesional` | `uuid` | Primary Key | FK `usuarios.id_usuario` Not Null Unique | Vinculación con usuario |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `especialidad` | `TipoEspecialidad` | Nullable | Especialidad médica/psicológica |
| `matricula` | `text` | Nullable | Matrícula profesional |
| `cuit` | `text` | Nullable | CUIT/CUIL del profesional para ARCA |
| `precio_consulta` | `numeric(10,2)` | Nullable | Valor base de la consulta |
| `tipo_comprobante` | `TipoComprobante` | Nullable | Tipo de factura emitida |
| `condicion_iva` | `CondicionIVA` | Nullable | Condición frente al IVA |

## Table `pacientes`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_paciente` | `int8` | Primary Identity | FK `personas.id_persona` Not Null | Vinculación con datos de la persona |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `num_documento` | `text` | Nullable | Número de DNI / CUIL / Pasaporte (alfanumérico) |
| `tipo_documento` | `TipoDocumento` | Nullable | Tipo de documento |
| `id_obra_social` | `int8` | FK `obras_sociales.id_obra_social` Not Null | Obra social del paciente ('Particular' por defecto) |
| `num_obra_social` | `text` | Nullable | Número de afiliado a la obra social |
| `porcentaje_iva` | `PorcentajeIVA` | Nullable | Porcentaje de IVA aplicable |

## Table `turnos`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_turno` | `int8` | Primary Identity | Clave primaria |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `id_paciente` | `int8` | FK `pacientes.id_paciente` Not Null | Paciente asignado |
| `id_profesional` | `uuid` | FK `profesionales.id_profesional` Not Null | Profesional asignado |
| `fecha` | `date` | Not Null | Fecha del turno |
| `hora` | `time` | Not Null | Hora del turno |
| `estado` | `EstadoTurno` | Default `'Activo'` | Estado (`Activo`, `Completo`, `Falta`, `Cancelado`) |
| `observacion` | `text` | Nullable | Notas u observaciones de la consulta |

## Table `obras_sociales`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_obra_social` | `int8` | Primary Identity | Clave primaria |
| `created_at` | `timestamptz` | Default `now()` | Fecha de creación |
| `nombre` | `text` | Not Null Unique | Nombre de la obra social / prepaga |
| `activo` | `bool` | Default `true` | Estado del registro |

## Table `agendas_compartidas`

| Name | Type | Constraints | Descripción |
| --- | --- | --- | --- |
| `id_administrativo` | `uuid` | FK `usuarios.id_usuario` PK | Id del usuario administrativo |
| `id_profesional` | `uuid` | FK `profesionales.id_profesional` PK | Id del profesional cuya agenda gestiona |
| `created_at` | `timestamptz` | Default `now()` | Fecha de asignación del permiso |

---

# Índices y Restricciones de Integridad

### Prevención de Solapamiento de Turnos (Índice de Exclusión Parcial)
Garantiza a nivel de motor PostgreSQL/Supabase que no puedan registrarse dos turnos activos (no cancelados) para un mismo profesional en la misma fecha y hora:

```sql
CREATE UNIQUE INDEX idx_turnos_profesional_fecha_hora_unicos 
ON turnos (id_profesional, fecha, hora) 
WHERE estado != 'Cancelado';
```

---

# Custom Types / Enums

### `RolUsuario`
`Administrativo` | `Profesional` | `Operador`

### `CondicionIVA`
`ConsumidorFinal` | `Monotributo` | `ResponsableInscripto` | `Exento`

### `EstadoTurno`
`Activo` | `Completo` | `Falta` | `Cancelado`

### `PorcentajeIVA`
`-1` | `0` | `10.5` | `21`

### `TipoComprobante`
`FacturaB` | `FacturaA`

### `TipoDocumento`
`DNI` | `CUIL` | `Pasaporte`

### `TipoEspecialidad`
`Psicologo` | `Psiquiatra` | `Otro`

---

# Estrategia de Facturación y Comprobantes
La facturación en el sistema Facturno se gestiona **únicamente mediante la generación de archivos PDF al vuelo** (y/o integración con ARCA sin persistencia de tabla de comprobantes en la DB). No se requiere una tabla de facturas/comprobantes en Supabase.

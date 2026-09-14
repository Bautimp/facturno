Los casos de uso identificados dentro del alcance definido para el Trabajo Final del sistema Facturno son los siguientes:

### Autenticación y Acceso (Compartido - Usuarios)
- **CU01**: Iniciar Sesión
- **CU02**: Cerrar Sesión
- **CU03**: Recuperar Contraseña

### Gestión de Pacientes (Unidos en actor Usuario: Profesional y Administrativo)
- **CU04**: Registrar / Agregar Paciente
- **CU05**: Modificar Paciente
- **CU06**: Gestionar Obra Social / Cobertura *(Caso de uso reincorporado y corregido)*
- **CU07**: Consultar Información de Paciente
- **CU08**: Ver Historial de Paciente *(Extensión de CU07)*

### Gestión de Agenda y Turnos (Unidos en actor Usuario: Profesional y Administrativo)
- **CU09**: Ver Agenda
- **CU10**: Agregar Turno
- **CU11**: Modificar Turno (Reagendamiento y Cambio de Estado)
- **CU12**: Cancelar / Eliminar Turno

### Facturación y Honorarios (Exclusivo Profesional)
- **CU13**: Facturar Sesión (Integración ARCA / Factura PDF)
- **CU14**: Modificar Precio de Consulta y Datos Fiscales

### Gestión de Personal y Permisos (Exclusivo Operador)
- **CU15**: Agregar Profesional
- **CU16**: Modificar Profesional
- **CU17**: Baja Lógica de Profesional
- **CU18**: Agregar Administrativo
- **CU19**: Modificar Administrativo
- **CU20**: Baja Lógica de Administrativo
- **CU21**: Vincular Agenda de Profesional a Administrativo

---

### Nota sobre Alcance y Organización
- **Unificación de Casos Comunes**: Los casos de uso compartidos por Profesional y Administrativo (Gestión de Pacientes y Agenda) se vinculan directamente al actor abstracto **Usuario**, del cual heredan ambos roles.
- **CU06 (Resuelto)**: Queda definido explícitamente como **Gestionar Obra Social / Cobertura**, corrigiendo la omisión previa.
- **Estados de Turno Sincronizados con Base de Datos**: `Activo`, `Completo`, `Falta`, `Cancelado`.
- **Autoregistro Público Descartado**: El alta de usuarios (`Profesional`, `Administrativo`) es 100% centralizada y realizada exclusivamente por el `Operador`. Se elimina la opción de registro abierto.
- **Notificaciones Simplificadas**: Las acciones de notificar o informar ausencias se realizan a través de la actualización del estado del turno (`Ausente`, `Cancelado`, `Finalizado`) y las observaciones de la agenda, sin almacenamiento de mensajes en una tabla de notificaciones.
- **Facturación al Vuelo**: La emisión de comprobantes fiscales se maneja mediante la generación de archivos PDF al vuelo sin persistencia de tabla de comprobantes en la DB.

---

### Diagrama de Casos de Uso (PlantUML)

```PlantUML
@startuml

left to right direction
skinparam packageStyle rectangle

' --- Actores ---
' Actor base que agrupa las funciones generales y tareas compartidas
actor "Usuarios" as pa

actor "Profesional" as p
actor "Administrativo" as a
actor "Operador" as o

' --- Jerarquía de Actores ---
' Profesional, Administrativo y Operador heredan de Usuarios
o <|-- pa
pa <|-- p
pa <|-- a

' --- Paquete: Gestión de Personal (Operador) ---
package "Gestión de Personal (Operador)" {
  usecase "CU18: Agregar Profesional" as uc31
  usecase "CU19: Modificar profesional" as uc32
  usecase "CU20: Baja lógica profesional" as uc33
  usecase "CU21: Agregar administrativo" as uc34
  usecase "CU22: Modificar administrativo" as uc35
  usecase "CU23: Baja lógica administrativo" as uc36
  usecase "CU24: Vincular agenda de profesional\na administrativo" as uc37
}

' --- Paquete: Gestión de Pacientes y Agenda (Compartido) ---
package "Gestión de Pacientes y Agenda" {
  usecase "CU1: Registrarse" as uc01
  usecase "CU2: Iniciar Sesión" as uc02
  usecase "CU3: Recuperar contraseña" as uc03
  usecase "CU4: Cerrar Sesión" as uc04
  usecase "CU5: Agregar un paciente" as uc11
  usecase "CU6: Agregar obra social" as uc19
  usecase "CU7: Modificar un usuario" as uc12
  usecase "Modificar obra social" as uc110
  usecase "CU8: Agregar un turno" as uc14
  usecase "CU9: Modificar un turno" as uc15
  usecase "CU10: Eliminar turno" as uc16
  usecase "CU11: Ver info paciente" as uc17
  usecase "Ver historial paciente" as uc111
  usecase "CU12: Ver agenda" as uc112
}


pa -- uc01
pa -- uc02
pa -- uc03
pa -- uc04

' Relaciones de Extensión
uc11 ..> uc19 : <<extend>>
uc12 ..> uc110 : <<extend>>
uc17 ..> uc111 : <<extend>>

' Asociaciones de gestión compartidas para el actor base (Usuarios)
pa -- uc11
pa -- uc12
pa -- uc14
pa -- uc15
pa -- uc16
pa -- uc17
pa -- uc112

' --- Paquete: Exclusivo del Profesional ---
package "Facturación y Precio (Profesional)" {
  usecase "CU13: Facturar una sesión" as uc13
  usecase "CU14: Modificar precio de consulta" as uc18
}

' Solo el Profesional accede a estas funciones
p -- uc13
p -- uc18

' --- Paquete: Tareas Administrativas ---
package "Tareas Administrativas y Notificaciones" {
  usecase "CU15: Notificar profesional" as uc21
  usecase "CU16: Informar ausencia de paciente" as uc22
  usecase "CU17: Solicitar factura" as uc23
}

' Solo el Administrativo accede a estas funciones
a -- uc21
a -- uc22
a -- uc23



' Solo el Operador accede a estas funciones
o -- uc31
o -- uc32
o -- uc33
o -- uc34
o -- uc35
o -- uc36
o -- uc37

@enduml
```
